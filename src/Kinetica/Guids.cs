// Guids.cs
// MUST match guids.h

using System;

namespace ActiveMesa.Kinetica
{
  static class GuidList
  {
    public const string guidKineticaPkgString = "dde39843-2019-48e9-afa6-f279a0465efa";
    public const string guidKineticaCmdSetString = "0642e6cb-60c5-4d57-aae3-e971fa85b6dd";

    public static readonly Guid guidKineticaCmdSet = new Guid(guidKineticaCmdSetString);
  };
}