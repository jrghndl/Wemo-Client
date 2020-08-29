Imports System.Net
Imports System.Net.NetworkInformation
Imports System.Threading

Public Class NetworkScanner
    Private DefaultGateway As IPAddress
    Private SubnetMask As IPAddress
    Private NetworkDevices As List(Of NetworkDevice)
    Private WemoDevices As List(Of WemoDevice)

    'Performs the device scan.
    Public Sub Scan()
        Console.WriteLine("Scanning network for Wemo devices. This may take a bit.")

        NetworkDevices = New List(Of NetworkDevice)
        WemoDevices = New List(Of WemoDevice)

        Try
            GetNetworkInfo()
            'Console.WriteLine("Default Gateway: " & DefaultGateway.ToString)
            'Console.WriteLine("Subnet Mask: " & SubnetMask.ToString)

            PingAllDevices()

            'Console.WriteLine(vbCrLf & "Network Devices:")
            'For Each Device As NetworkDevice In NetworkDevices
            '    Device.DisplayInfo()
            'Next

            ListWemoDevices()
        Catch ex As Exception
            Console.WriteLine("An error has occurred during the scan. Please restart the scan.")
        End Try
    End Sub

    'Gets the default gateway address and the subnet mask.
    Private Sub GetNetworkInfo()
        For Each ip In System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList
            If ip.AddressFamily = Net.Sockets.AddressFamily.InterNetwork Then
                For Each adapter As Net.NetworkInformation.NetworkInterface In Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
                    For Each unicastIPAddressInformation As Net.NetworkInformation.UnicastIPAddressInformation In adapter.GetIPProperties().UnicastAddresses
                        If unicastIPAddressInformation.Address.AddressFamily = Net.Sockets.AddressFamily.InterNetwork Then
                            If ip.Equals(unicastIPAddressInformation.Address) Then
                                SubnetMask = unicastIPAddressInformation.IPv4Mask
                                Dim adapterProperties As Net.NetworkInformation.IPInterfaceProperties = adapter.GetIPProperties()
                                DefaultGateway = adapterProperties.GatewayAddresses(0).Address
                            End If
                        End If
                    Next
                Next
            End If
        Next
    End Sub

    'Generates a list of possible IP Addresses using the default gateway address and subnet mask.
    'Then asynchronously pings each IP on the list. For each IP a response is received from,
    'it creates a new NetworkDevice object and adds it to the NetworkDevices list.
    Private Sub PingAllDevices()
        Dim mask() As Byte = SubnetMask.GetAddressBytes
        Dim iprev() As Byte = DefaultGateway.GetAddressBytes
        Dim netid() As Byte = BitConverter.GetBytes(BitConverter.ToUInt32(iprev, 0) And BitConverter.ToUInt32(mask, 0)) ' Network id - network address
        Dim inv_mask() As Byte = mask.Select(Function(r) Not r).ToArray ' Binary inverted netmask
        Dim brCast() As Byte = BitConverter.GetBytes(BitConverter.ToUInt32(netid, 0) Xor BitConverter.ToUInt32(inv_mask, 0)) ' Broadcast address

        Dim IPList As New List(Of String)
        For n As UInt32 = BitConverter.ToUInt32(netid.Reverse.ToArray, 0) + 1 To BitConverter.ToUInt32(brCast.Reverse.ToArray, 0) - 1
            IPList.Add(New Net.IPAddress(BitConverter.GetBytes(n).Reverse.ToArray).ToString)
        Next

        Dim Pinger As New Ping
        Dim Tasks = New List(Of Task)()
        For Each IP As String In IPList
            Tasks.Add(PingDevice(IP))
        Next
        Task.WhenAll(Tasks).GetAwaiter().GetResult()
        Thread.Sleep(1000)
    End Sub

    'Asynchronous function that pings the device at the given ip. Also gets the physical address for each device.
    Private Async Function PingDevice(ByVal IP As String) As Task
        Dim Pinger As New Ping
        Dim Reply = Await Pinger.SendPingAsync(IP)
        For x = 1 To 5
            If Reply.Status = 0 Then
                Dim Device As New NetworkDevice(Reply.Address)
                NetworkDevices.Add(Device)
                While (Device.MACAddress = "")
                    Await Pinger.SendPingAsync(IP)
                    Await Device.GetMAC()
                End While
                For i As UInteger = 49152 To 49153 'Most common Wemo ports.
                    If (Await Device.CheckWemoDevice(IP, i)) Then
                        WemoDevices.Add(New WemoDevice(Device.IP, Device.MACAddress, i))
                        Exit For
                    End If
                Next
                Exit For
            Else
                Reply = Await Pinger.SendPingAsync(IP)
            End If
        Next

    End Function

    'Returns a specific Wemo device.
    Function GetWemoDevice(ByVal Number As Integer) As WemoDevice
        Return WemoDevices(Number)
    End Function

    'Returns the total count of Wemo devices on the network.
    Function GetTotalWemoDevices() As Integer
        Return WemoDevices.Count
    End Function

    'Outputs a list of all Wemo devices.
    Public Sub ListWemoDevices()
        Console.WriteLine(vbCrLf & "Wemo Devices:")
        For x = 0 To WemoDevices.Count - 1
            Console.WriteLine(x & ": " & WemoDevices(x).IP.ToString)
        Next
        Console.WriteLine(vbCrLf)
    End Sub
End Class
