﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Advobot.Invites.Resources {
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
    internal class Responses {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Responses() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Advobot.Invites.Resources.Responses", typeof(Responses).Assembly);
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
        ///   Looks up a localized string similar to Successfully bumped the listed invite..
        /// </summary>
        internal static string ListedInviteBumped {
            get {
                return ResourceManager.GetString("ListedInviteBumped", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Successfully created a listed invite from {0}..
        /// </summary>
        internal static string ListedInviteCreated {
            get {
                return ResourceManager.GetString("ListedInviteCreated", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Successfully deleted the listed invite..
        /// </summary>
        internal static string ListedInviteDeleted {
            get {
                return ResourceManager.GetString("ListedInviteDeleted", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to find an invite with the supplied options..
        /// </summary>
        internal static string ListedInviteNoInviteMatch {
            get {
                return ResourceManager.GetString("ListedInviteNoInviteMatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to find a suitable invite; too many were found..
        /// </summary>
        internal static string ListedInviteTooManyMatches {
            get {
                return ResourceManager.GetString("ListedInviteTooManyMatches", resourceCulture);
            }
        }
    }
}