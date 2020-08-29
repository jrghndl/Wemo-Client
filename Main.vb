Module Main

    Sub Main()
        Dim NetworkScanner As New NetworkScanner
        NetworkScanner.Scan()
        Do
            Try
                'User input handler.
                Dim Input() As String = Console.ReadLine.Split(" ")
                Select Case Input(0).ToLower
                    Case "wemo"
                        If Input(1) = "*" Then
                            If Input(2).ToLower = "on" Then
                                For x = 0 To NetworkScanner.GetTotalWemoDevices - 1
                                    NetworkScanner.GetWemoDevice(x).TurnOn()
                                Next
                            ElseIf Input(2).ToLower = "off" Then
                                For x = 0 To NetworkScanner.GetTotalWemoDevices - 1
                                    NetworkScanner.GetWemoDevice(x).TurnOff()
                                Next
                            ElseIf Input(2).ToLower = "status" Then
                                For x = 0 To NetworkScanner.GetTotalWemoDevices - 1
                                    Console.WriteLine(x & ": " & NetworkScanner.GetWemoDevice(x).GetStatus())
                                Next
                                Console.WriteLine(vbCrLf)
                            Else
                                Console.WriteLine("Unrecognized Input")
                            End If
                        ElseIf Input(1).ToLower = "list" Then
                            NetworkScanner.ListWemoDevices()
                        ElseIf Input(1).ToLower = "scan" Then
                            NetworkScanner.Scan()
                        Else
                            If Input(2).ToLower = "on" Then
                                NetworkScanner.GetWemoDevice(Input(1)).TurnOn()
                            ElseIf Input(2).ToLower = "off" Then
                                NetworkScanner.GetWemoDevice(Input(1)).TurnOff()
                            ElseIf Input(2).ToLower = "status" Then
                                Console.WriteLine(Input(1) & ": " & NetworkScanner.GetWemoDevice(Input(1)).GetStatus())
                            Else
                                Console.WriteLine("Unrecognized Input")
                            End If
                        End If
                    Case "clear"
                        Console.Clear()
                    Case "exit"
                        Exit Do
                    Case Else
                        Console.WriteLine("Unrecognized Command")
                End Select
            Catch ex As Exception
                Console.WriteLine("Input Error")
            End Try
        Loop
    End Sub
End Module

