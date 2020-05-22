Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Public Class NetboardClient_v1
    Dim DefaultSendToIP As String = "eqoa.ddns.net"
    Dim DefaultPorts As Integer = "51649"
    Dim Newline = vbNewLine
    Dim ChatThrottleTimer As Integer = 0
    Dim Kaires As New Kaires_v1(Me, AddressOf DataReceived)

    'CS - CODE
    Dim LastClientIsListeningStatus As Boolean = Kaires.ClientIsListening
    Public ReadOnly Property MyIPv4 As IPAddress
        Get
            Return System.Net.Dns.GetHostByName(System.Net.Dns.GetHostName).AddressList(0)
        End Get
    End Property
    Public ReadOnly Property MyIPv6 As IPAddress
        Get
            Return System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName).AddressList(0)
        End Get
    End Property
    Private Function SendChatMessage(ByRef _chatMessage As String, Optional _clearData As Boolean = False)
        If ChatThrottleTimer > -1 Then Return -1 ' this should be done by tcp class later
        Button3.Enabled = False ' Send Message Button
        ChatThrottleTimer = 0
        Dim lasticon = NotifyIcon1.Icon
        NotifyIcon1.Icon = My.Resources.Yellow_Light_Icon_png
        Kaires.SendChatMessage(_chatMessage)
        NotifyIcon1.Icon = lasticon
        TextBox6.AppendText(">> You try to say:  " & _chatMessage & Newline)
        If _clearData Then _chatMessage = ""
        Return 0
    End Function ' Send Chat Message

    'CS - UI
    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Me.Location = New Point(250, 100)
        Label10.Text = "/" & TextBox5.MaxLength ' Your Next Message ' Max Length
        GroupBox1.Enabled = False ' Netbpard - Main
        Button4.Enabled = False ' Connect Button ' button
        Button5.Enabled = False ' Disconnect Button ' button
        TextBox7.Enabled = False ' Display Name ' textbox
        Label13.Enabled = False ' Display Name Label ' label
        TextBox8.Enabled = False ' Passcode ' textbox
        Label14.Enabled = False ' Passcode Label ' label
        NotifyIcon1.Icon = My.Resources.White_Light_Icon_png
        'Button16.PerformClick() ' RGC Button ' button
    End Sub 'Form Load
    Private Sub timer_UI_Tick(sender As System.Object, e As System.EventArgs) Handles timer_UI.Tick
        If LastClientIsListeningStatus <> Kaires.ClientIsListening Then 'if listening change occured..
            LastClientIsListeningStatus = Kaires.ClientIsListening
            If LastClientIsListeningStatus Then
                Label12.Text = "Listening" 'status label
                NotifyIcon1.Icon = My.Resources.Green_Light_Icon_png
            Else
                Label12.Text = "Not Listening" 'status label
                NotifyIcon1.Icon = My.Resources.Red_Light_Icon_png
            End If
        End If
        If ChatThrottleTimer > -1 Then
            If ChatThrottleTimer > 500 Then
                ChatThrottleTimer = -1
                Button3.Enabled = True ' Send Message Button
            Else
                Button3.Enabled = False ' Send Message Button
                ChatThrottleTimer += sender.Interval
            End If
        End If
    End Sub 'Status / UI Updater 'timer
    Private Sub TextBox5_TextChanged(sender As System.Object, e As System.EventArgs) Handles TextBox5.TextChanged
        'allow right-clicking the label to change to a counting-down mode instead of a counting-up mode.
        Label9.Text = sender.Text.Length
    End Sub 'Updates Used Character Count
    Private Sub TextBox5_KeyDown(sender As System.Object, e As System.Windows.Forms.KeyEventArgs) Handles TextBox5.KeyDown
        If e.KeyCode = Keys.Enter Then
            SendChatMessage(TextBox5.Text, True)
        End If
    End Sub 'send message 'enter pressed
    Private Sub Button3_Click(sender As System.Object, e As System.EventArgs) Handles Button3.Click
        SendChatMessage(TextBox5.Text, True)
    End Sub 'send message 'button pressed

    'CLIENT - CODE
    Public Sub DataReceived(ByVal _dataType As String, ByVal _dataObject As Object)
        Select Case _dataType
            Case "console message"
                TextBox6.AppendText(" >> " & _dataObject & " << " & Newline)
            Case "chat message"
                If _dataObject(0) = TextBox7.Text Then
                    TextBox6.AppendText("You say: " & _dataObject(1) & Newline)
                Else
                    TextBox6.AppendText(_dataObject(0) & " says: " & _dataObject(1) & Newline)
                End If
            Case "chat alert"
                    TextBox6.AppendText(_dataObject(1) & Newline) ' while server show as Server
            Case "game update"
                    TextBox6.AppendText("GU :: " & _dataObject(0) & " IA -> " & _dataObject(1) & ", " & _dataObject(2) & ", " & _dataObject(3) & Newline)
            Case Else
                    TextBox6.AppendText(Me.Name & " has receieved an unknown _dataType in DataReceived()")
        End Select
    End Sub 'Data Received

    'CLIENT - UI
    Private Sub Button16_Click(sender As System.Object, e As System.EventArgs) Handles Button16.Click
        sender.Enabled = False ' RGC Button ' button
        Button4.Enabled = True ' Connect Button ' button
        TextBox7.Enabled = True ' Display Name ' textbox
        Label13.Enabled = True ' Display Name Label ' label
        TextBox8.Enabled = True ' Passcode ' textbox
        Label14.Enabled = True ' Passcode Label ' label
        timer_UI.Start()
    End Sub 'RGC Button
    Private Sub Button4_Click(sender As System.Object, e As System.EventArgs) Handles Button4.Click
        '' Display Name Correctness Check ' Should be pushed to serverside
        Dim originaltext As String = TextBox7.Text
        TextBox7.Text = TextBox7.Text.Trim.Replace(" ", "")
        If originaltext <> TextBox7.Text Then Exit Sub
        If TextBox7.Text.Length < 5 Then Exit Sub
        ''
        GroupBox1.Enabled = True ' Netbpard - Main
        Button4.Enabled = False ' Connect Button ' button
        Button5.Enabled = True ' Disconnect Button ' button
        TextBox7.Enabled = False ' Display Name ' textbox
        Label13.Enabled = False ' Display Name Label ' label
        TextBox8.Enabled = False ' Passcode ' textbox
        Label14.Enabled = False ' Passcode Label ' label

        If TextBox8.Text <> "" Then DefaultSendToIP = TextBox8.Text
        If Kaires.ConnectToServer(DefaultSendToIP, DefaultPorts, TextBox7.Text, TextBox8.Text, MyIPv4, DefaultPorts) <> 0 Then Button5.PerformClick()
    End Sub ' Connect Button
    Private Sub Button5_Click(sender As System.Object, e As System.EventArgs) Handles Button5.Click
        GroupBox1.Enabled = False
        Button4.Enabled = True
        Button5.Enabled = False
        TextBox7.Enabled = True
        Label13.Enabled = True
        TextBox8.Enabled = True
        Label14.Enabled = True
        Kaires.StopListening()
    End Sub ' Disconnect Button

    'none
    Private Sub Label14_Click(sender As System.Object, e As System.EventArgs) Handles Label14.Click
        If TextBox8.Text = "" Then
            TextBox8.Text = "192.168.0.9"
        ElseIf TextBox8.Text = "192.168.0.9" Then
            TextBox8.Text = "192.168.0.150"
        Else
            TextBox8.Text = ""
        End If
    End Sub
    Private Sub Label13_Click(sender As System.Object, e As System.EventArgs) Handles Label13.Click
        TextBox7.Text = "bobby"
    End Sub
End Class
