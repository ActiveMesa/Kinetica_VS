using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;

namespace ActiveMesa.Kinetica
{
	/// <summary>
	/// This is the class that implements the package exposed by this assembly.
	///
	/// The minimum requirement for a class to be considered a valid package for Visual Studio
	/// is to implement the IVsPackage interface and register itself with the shell.
	/// This package uses the helper classes defined inside the Managed Package Framework (MPF)
	/// to do it: it derives from the Package class that provides the implementation of the 
	/// IVsPackage interface and uses the registration attributes defined in the framework to 
	/// register itself and its components with the shell.
	/// </summary>
	// This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
	// a package.
	[PackageRegistration(UseManagedResourcesOnly = true)]
	// This attribute is used to register the information needed to show this package
	// in the Help/About dialog of Visual Studio.
	[InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
	// This attribute is needed to let the shell know that this package exposes some menus.
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(GuidList.guidKineticaPkgString)]
	public sealed class KineticaPackage : Package
	{
		private readonly Stopwatch st = new Stopwatch();
		private readonly SortedDictionary<long, string> snapshots = new SortedDictionary<long, string>();
		private TextEditorEvents textEditorEvents;
		private IVsTextManager textManager;
		private IVsEditorAdaptersFactoryService eafs;

		/// <summary>
		/// Default constructor of the package.
		/// Inside this method you can place any initialization code that does not require 
		/// any Visual Studio service because at this point the package object is created but 
		/// not sited yet inside Visual Studio environment. The place to do all the other 
		/// initialization is the Initialize method.
		/// </summary>
		public KineticaPackage()
		{
			Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", ToString()));
		}

		protected override void Dispose(bool disposing)
		{
			st.Stop();
			base.Dispose(disposing);
		}

		/// <summary>
		/// Initialization of the package; this method is called right after the package is sited, so this is the place
		/// where you can put all the initialization code that rely on services provided by VisualStudio.
		/// </summary>
		protected override void Initialize()
		{
			var cm = (IComponentModel)GetGlobalService(typeof(SComponentModel));
			eafs = cm.GetService<IVsEditorAdaptersFactoryService>();

			Debug.WriteLine(string.Format(CultureInfo.CurrentCulture,
				"Entering Initialize() of: {0}", ToString()));
			base.Initialize();

			// Add our command handlers for menu (commands must exist in the .vsct file)
			OleMenuCommandService mcs =
				GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
			if (null != mcs)
			{
				// Create the command for the menu item.
				CommandID menuCommandID = new CommandID(GuidList.guidKineticaCmdSet, 
					(int)PkgCmdIDList.cmdStartTracking);
				MenuCommand menuItem = new MenuCommand(StartTracking, menuCommandID);
				mcs.AddCommand(menuItem);

				// command for saving the snapshot to a folder
				var id = new CommandID(GuidList.guidKineticaCmdSet, 
					(int) PkgCmdIDList.cmdSaveSnapshot);
				var mi = new MenuCommand(SaveSnapshots, id);
				mcs.AddCommand(mi);
			}

			// listen to document changes
			var dte = (DTE)GetService(typeof(DTE));
			textEditorEvents = dte.Events.TextEditorEvents;
			textEditorEvents.LineChanged += (point, endPoint, hint) =>
			{
				var elapsed = st.ElapsedMilliseconds;
				var app = (DTE)GetService(typeof(SDTE));
				if (app.ActiveDocument != null && app.ActiveDocument.Type == "Text")
				{
					//// or the view
					//var txtMgr = (IVsTextManager) GetService(typeof (SVsTextManager));
					//IVsTextView view;
					//txtMgr.GetActiveView(1, null, out view);

					IVsTextView view;
					textManager.GetActiveView(1, null, out view);
					var wpfView = eafs.GetWpfTextView(view);

					var doc = (TextDocument)app.ActiveDocument.Object();
					var ep = doc.CreateEditPoint(doc.StartPoint);
				  string txt = ep.GetText(doc.EndPoint);
					
          // fixme: hashcode collisions WILL happen here
					snapshots.Add(st.ElapsedMilliseconds, txt);
				}
			};

			st.Start();

			// get the text manager
			textManager = (IVsTextManager)base.GetService(typeof(SVsTextManager));
		}

		private void SaveSnapshots(object sender, EventArgs e)
		{
      
			var dlg = new FolderBrowserDialog { ShowNewFolderButton = true };
			if (dlg.ShowDialog() == DialogResult.OK)
			{
				var path = dlg.SelectedPath;
				foreach (var entry in snapshots)
				{
					File.WriteAllText(Path.Combine(path, entry.Key + ".txt"), entry.Value);
				}
				snapshots.Clear();
			}
		}

		/// <summary>
		/// This function is the callback used to execute a command when the a menu item is clicked.
		/// See the Initialize method to see how the menu item is associated to this function using
		/// the OleMenuCommandService service and the MenuCommand class.
		/// </summary>
		private void StartTracking(object sender, EventArgs e)
		{
			// Show a Message Box to prove we were here
			//var app = (EnvDTE.DTE) GetService(typeof (SDTE));

			//if (app.ActiveDocument != null && app.ActiveDocument.Type == "Text")
			//{

			//  var doc = (EnvDTE.TextDocument) app.ActiveDocument.Object();
			//  var text = doc.CreateEditPoint(doc.StartPoint).GetText(doc.EndPoint);
			//}
		}

	}
}
