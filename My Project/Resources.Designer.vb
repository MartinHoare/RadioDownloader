﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:2.0.50727.312
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On

Imports System

Namespace My.Resources
    
    'This class was auto-generated by the StronglyTypedResourceBuilder
    'class via a tool like ResGen or Visual Studio.
    'To add or remove a member, edit your .ResX file then rerun ResGen
    'with the /str option, or rebuild your VS project.
    '''<summary>
    '''  A strongly-typed resource class, for looking up localized strings, etc.
    '''</summary>
    <Global.System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0"),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute(),  _
     Global.Microsoft.VisualBasic.HideModuleNameAttribute()>  _
    Friend Module Resources
        
        Private resourceMan As Global.System.Resources.ResourceManager
        
        Private resourceCulture As Global.System.Globalization.CultureInfo
        
        '''<summary>
        '''  Returns the cached ResourceManager instance used by this class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend ReadOnly Property ResourceManager() As Global.System.Resources.ResourceManager
            Get
                If Object.ReferenceEquals(resourceMan, Nothing) Then
                    Dim temp As Global.System.Resources.ResourceManager = New Global.System.Resources.ResourceManager("RadioDld.Resources", GetType(Resources).Assembly)
                    resourceMan = temp
                End If
                Return resourceMan
            End Get
        End Property
        
        '''<summary>
        '''  Overrides the current thread's CurrentUICulture property for all
        '''  resource lookups using this strongly typed resource class.
        '''</summary>
        <Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
        Friend Property Culture() As Global.System.Globalization.CultureInfo
            Get
                Return resourceCulture
            End Get
            Set
                resourceCulture = value
            End Set
        End Property
        
        '''<summary>
        '''  Looks up a localized string similar to body { 
        '''	background-color: #3F3F3F;
        '''	font: 8pt verdana;
        '''	color: white;
        '''}
        '''
        '''html, body, table {
        '''	height: 100%;
        '''	margin: 0px;
        '''}
        '''
        '''h1 {
        '''	font-size: 10pt;
        '''	margin-bottom: 12px;
        '''	padding-left: 5px;
        '''}
        '''
        '''h2 {
        '''	color: #999999;
        '''	font-size: 8pt;
        '''	margin-top: 0px;
        '''	margin-bottom: 5px;
        '''}
        '''
        '''table {
        '''	width: 100%;
        '''	border-collapse: collapse;
        '''	margin: 0px;
        '''}
        '''
        '''td {
        '''	vertical-align: top;
        '''	margin: 0px;	
        '''}
        '''
        '''.maintd {
        '''	padding: 10px;
        '''}
        '''
        '''.bottomrow {
        '''	vertical-align: bottom;
        '''	padding-top: 0px [rest of string was truncated]&quot;;.
        '''</summary>
        Friend ReadOnly Property SIDEBAR_CSS() As String
            Get
                Return ResourceManager.GetString("SIDEBAR_CSS", resourceCulture)
            End Get
        End Property
    End Module
End Namespace
