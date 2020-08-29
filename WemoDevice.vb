Imports System.IO
Imports System.Net
Imports System.Text
Imports System.Xml

Public Class WemoDevice
    Inherits NetworkDevice
    Public Property Port As UInteger
    Private OnRequest As String = "<?xml version=""1.0"" encoding=""utf-8""?><s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/""><s:Body><u:SetBinaryState xmlns:u=""urn:Belkin:service:basicevent:1""><BinaryState>1</BinaryState></u:SetBinaryState></s:Body></s:Envelope>"
    Private OffRequest As String = "<?xml version=""1.0"" encoding=""utf-8""?><s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/""><s:Body><u:SetBinaryState xmlns:u=""urn:Belkin:service:basicevent:1""><BinaryState>0</BinaryState></u:SetBinaryState></s:Body></s:Envelope>"
    Private GetRequest As String = "<?xml version=""1.0"" encoding=""utf-8""?><s:Envelope xmlns:s=""http://schemas.xmlsoap.org/soap/envelope/"" s:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/""><s:Body><u:GetBinaryState xmlns:u=""urn:Belkin:service:basicevent:1""></u:GetBinaryState></s:Body></s:Envelope>"

    'Initializes the WemoDevice object.
    Sub New(ByVal IP As IPAddress, ByVal MAC As String, ByVal Port As UInteger)
        MyBase.New(IP)
        Me.Port = Port
    End Sub

    'Turns the Wemo device on.
    Public Sub TurnOn()
        SendSetRequest(OnRequest)
    End Sub

    'Turns the Wemo device off.
    Public Sub TurnOff()
        SendSetRequest(OffRequest)
    End Sub

    'Sends the web request that turns the Wemo device on or off.
    Private Sub SendSetRequest(ByVal RequestType As String)
        Dim Request As HttpWebRequest = HttpWebRequest.Create(GetRequestURL)
        Request.Method = "POST"
        Request.Headers.Add("SOAPAction", """urn:Belkin:service:basicevent:1#SetBinaryState""")
        Request.ContentType = "text/xml; charset=""utf-8"""
        Request.KeepAlive = False
        Dim Bytes As Byte() = UTF8Encoding.ASCII.GetBytes(RequestType)
        Request.ContentLength = Bytes.Length
        Dim Stream As Stream = Request.GetRequestStream()
        Using (Stream)
            Stream.Write(Bytes, 0, Bytes.Length)
            Stream.Close()
            Request.GetResponse()
        End Using
        Request.Abort()
    End Sub

    'Returns the address where web requests must be sent to.
    Private Function GetRequestURL()
        Return $"http://{IP}:{Port}/upnp/control/basicevent1"
    End Function

    'Gets whether the Wemo device is on or off.
    Public Function GetStatus() As String
        Dim Request As HttpWebRequest = HttpWebRequest.Create(GetRequestURL)
        Dim Response As HttpWebResponse
        Request.Method = "POST"
        Request.Headers.Add("SOAPAction", """urn:Belkin:service:basicevent:1#GetBinaryState""")
        Request.ContentType = "text/xml; charset=""utf-8"""
        Request.KeepAlive = False
        Dim Bytes As Byte() = UTF8Encoding.ASCII.GetBytes(GetRequest)
        Request.ContentLength = Bytes.Length
        Dim Stream As Stream = Request.GetRequestStream()
        Using (Stream)
            Stream.Write(Bytes, 0, Bytes.Length)
            Stream.Close()
            Response = Request.GetResponse()
        End Using
        Dim ResponseStreamReader As StreamReader = New StreamReader(Response.GetResponseStream)
        Dim ResponseText As String = ResponseStreamReader.ReadToEnd
        Request.Abort()
        Dim XML = New XmlDocument()
        XML.LoadXml(ResponseText)
        If (XML.ChildNodes(0).ChildNodes(0).ChildNodes(0).InnerText = 0) Then
            Return "Off"
        Else
            Return "On"
        End If
    End Function

End Class

