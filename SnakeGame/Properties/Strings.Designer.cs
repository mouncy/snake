﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SnakeGame.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SnakeGame.Properties.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to To play again.
        /// </summary>
        internal static string RestartText_01 {
            get {
                return ResourceManager.GetString("RestartText_01", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to press space.
        /// </summary>
        internal static string RestartText_02 {
            get {
                return ResourceManager.GetString("RestartText_02", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to To resume press space.
        /// </summary>
        internal static string ResumeText {
            get {
                return ResourceManager.GetString("ResumeText", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Use the arrow keys.
        /// </summary>
        internal static string StartText_01 {
            get {
                return ResourceManager.GetString("StartText_01", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to or w, a, s, d keys to move..
        /// </summary>
        internal static string StartText_02 {
            get {
                return ResourceManager.GetString("StartText_02", resourceCulture);
            }
        }
    }
}
