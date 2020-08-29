Imports System.Net

Public Class NetworkDevice
    Public IP As IPAddress
    Public MACAddress As String
    Declare Function SendARP Lib "iphlpapi.dll" (ByVal DestIP As UInt32, ByVal SrcIP As UInt32, ByVal pMacAddr As Byte(), ByRef PhyAddrLen As Integer) As Integer

    'Initializes the NetworkDevice object.
    Public Sub New(ByVal IP As IPAddress)
        Me.IP = IP
    End Sub

    'Gets the MAC for a given device using ARP
    Public Async Function GetMAC() As Task
        Dim mac() As Byte = New Byte(6) {}
        Dim len As Integer = mac.Length
        SendARP(CType(IP.Address, UInt32), 0, mac, len)
        MACAddress = BitConverter.ToString(mac, 0, len)
        Await Task.Delay(10)
    End Function

    'Overrides default WebClient
    Private Class MyWebClient
        Inherits WebClient
        Protected Overrides Function GetWebRequest(ByVal uri As Uri) As WebRequest
            Dim w As WebRequest = MyBase.GetWebRequest(uri)
            w.Timeout = 1000
            Return w
        End Function
    End Class

    Public Async Function CheckWemoDevice(ByVal ip As String, ByVal port As UInteger) As Task(Of Boolean)
        Dim url = $"http://{ip}:{port}/setup.xml"
        'Console.WriteLine(url)
        Try
            Using wc = New MyWebClient()

                Dim resp = Await wc.DownloadStringTaskAsync(url)
                If resp.Contains("Belkin") Then
                    Return True
                End If
            End Using

        Catch __unusedException1__ As Exception
            'Console.WriteLine(__unusedException1__)
        End Try
        Return False
    End Function

    'In case MAC returns empty, it pings the device again and gets the mac.
    Public Sub DisplayInfo()
        Console.WriteLine("IP: " & IP.ToString & " MAC: " & MACAddress)
    End Sub
End Class
