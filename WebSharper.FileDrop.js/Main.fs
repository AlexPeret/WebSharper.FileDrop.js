namespace WebSharper.FileDropJs

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Res =
    let FileDropJs = 
        Resource "DomJs" "filedrop-min.js" |> AssemblyWide

module Definition =

    let U = T<unit>
    let O = T<obj>
    let S = T<string>
    let B = T<bool>
    let I = T<int64>
    let F = T<float>
    let E = T<WebSharper.JavaScript.Dom.Element>
    let Ev = T<WebSharper.JavaScript.Dom.Event>
    let Args = !| O

    let File = Class "File"
    let FileList = Class "FileList"
    let EventMap = Class "EventMap"
    let EventHandlers = Class "EventHandlers"
    let DropHandle = Class "DropHandle"
    let FileDrop = Class "FileDrop"
    
    let InputSetupCfg =
        Class "InputSetupCfg"
        |+> Pattern.RequiredFields [
            "file", E
            "form", E
        ]

    //function (eventObject)
    let callbackEventObjectType = Ev ^-> U
    //function (DropHandle)
    let callbackDropHandleType = DropHandle ^-> U
    //function (DOM_Iframe)
    let callbackDomIframeEventType = E ^-> U
    //function (response)
    let callbackResponseType = T<WebSharper.JavaScript.XMLHttpRequestResponseType> ^-> U
    //function (files)
    let callbackFileListType = FileList ^-> U
    // function (xhr)
    let callbackXhrType = T<WebSharper.JavaScript.XMLHttpRequest> ^-> U
    let callbackFileType = File ^-> U
    //function (obj, event, args)
    let callbackEventXhrType = Ev * (!? T<WebSharper.JavaScript.XMLHttpRequest>) ^-> U
    let callbackXhrEventType = T<WebSharper.JavaScript.XMLHttpRequest> * Ev ^-> U
    let callback2FXhrEventType = F * F * T<WebSharper.JavaScript.XMLHttpRequest> * Ev ^-> U
    let callbackXhr2OType = T<WebSharper.JavaScript.XMLHttpRequest> * O * O ^-> U
    let callbackObjEventArgs = O * Ev * Args ^-> U
    //function ({ file: DOM_Input, form: DOM_Form }, oldFileInput)
    let callbackInputSetupType = InputSetupCfg * E ^-> U

    EventMap
    |+> Pattern.OptionalFields [
        "any", callbackObjEventArgs
        "dragEnter", callbackEventObjectType
        "dragLeave", callbackEventObjectType
        "dragOver", callbackEventObjectType
        "dragEnd", callbackEventObjectType
        "dragExit", callbackEventObjectType
        "upload", callbackEventObjectType
        "uploadElsewhere", callbackDropHandleType
        "inputSetup", callbackInputSetupType
        "iframeSetup", callbackDomIframeEventType
        "iframeDone", callbackResponseType
    ]
    |> ignore

    let FileOptions =
        Class "FileOptions"
        |+> Pattern.OptionalFields [
            "extraHeaders", S
            "xRequestedWith", B
            "method", S
        ]

    File
    |+> Instance [
        "sendTo" => S + (!? FileOptions) ^-> File
        // for "?"
        "event" => S * callbackXhrType ^-> U
        // for "xhrSend"
        "event" => S * callbackXhr2OType ^-> U
        // for "progress"
        "event" => S * callback2FXhrEventType ^-> U
        // for "done"
        "event" => S * callbackXhrEventType ^-> U
        // for "error"
        "event" => S * callbackEventXhrType ^-> U
    ]
    |> ignore

    FileList
    |+> Instance [
        "each" => callbackFileType ^-> FileList
    ]
    |> ignore

    let DropHandleCfg =
        Class "DropHandleCfg"
        |+> Pattern.OptionalFields [
            //"axis", AxesEnum.Type
            "zoneClass", !| DropHandle
            "inputClass", S
            "input", B + E
            "recreateInput", B
            "fullDocDragDetect", B
            "multiple", B
            "dropEffect", S
        ]

    DropHandle
    |+> Static [
        Constructor (S + E)
        Constructor ((S + E)?zone * DropHandleCfg?opt)
    ]
    |+> Instance [
        "el" =? E
        "filedrop" =? DropHandle
    ] 
    |+> Instance [
        // event()
        "event" => U ^-> EventMap
        // event(string)
        // event(string,[]:string)
        // event(string,[]:callback)
        // event([]:string)
        // event(eventMap)
        // event(string, null)
        // event([]:string, null)
        // event(string, callback)
        "event" => S * callbackFileListType ^-> DropHandle
        // event(string, []:callback)
        // event([]:string, callback)
        // event([]:string, []:callback)
        //"event" => ((S *+ !| S) * (!| S + callbackType + !| callbackType)) ^-> EventMap + EventHandlers
    ]
    |> ignore

    FileDrop
    |=> Inherits DropHandle
    |+> Static [
        Constructor (S + E)
        Constructor ((S + E)?zone * DropHandleCfg?opt)
    ]
    |+> Instance [
        "handle" =? DropHandle
        |> WithComment "Underlying DropHandle instance providing browser-independent handlers for drag & drop and <iframe> upload facility."
    ]
    |+> Instance [
        "eventFiles" => S?selector ^-> FileList + B
        |> WithComment "Retrieves File objects from an on-drop event. Returns a FileList array-like object (not W3C FileList). If orFalse is unset always returns a FileList even if event e was invalid, otherwise returns false in such occurrences instead of empty FileList."
        "eventFiles" => S?selector * B?orFalse ^-> FileList + B
    ] |> ignore

    let Assembly =
        Assembly [
            Namespace "WebSharper.FileDropJs.Resources" [ Res.FileDropJs ]
            Namespace "WebSharper.FileDropJs" [ 
                FileOptions
                File
                FileList
                InputSetupCfg
                EventMap
                EventHandlers
                DropHandleCfg
                DropHandle
                FileDrop
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()

