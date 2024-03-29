using System.Reflection;
using System.Runtime.InteropServices;
using MelonLoader;
using Quriz.CustomRadio;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Kebab Chefs! - Custom Radio Music Mod")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Quriz")]
[assembly: AssemblyProduct("KC_CustomRadio")]
[assembly: AssemblyCopyright("Copyright ©  2024")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("fb8d9dee-29ba-4b6f-8541-233606287fce")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: MelonInfo(typeof(Mod), "Custom Radio Music", "0.2.0", "Quriz")]
[assembly: MelonGame("Biotech Gameworks", "Kebab Chefs! - Restaurant Simulator")]
[assembly: MelonAdditionalDependencies("YoutubeDLSharp")]
