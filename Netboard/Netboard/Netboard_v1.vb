Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Public Class Netboard_v1
    Dim tcpReceiver As tcpReceiver_v1
    Dim tcpSender As tcpSender_v1
    Dim DefaultSendToIP As String = "eqoa.ddns.net"
    Dim DefaultPorts As Integer = "51649"
    Dim Newline = vbNewLine
    Dim ChatThrottleTimer As Integer = 0

    'CLIENT
    Dim LastClientIsListeningStatus As Boolean = Not ClientIsListening
    Private Property MyListeningIP As IPAddress
        Get
            If TextBox2.Text = "ANY" Then
                Return IPAddress.Any
            Else
                Return System.Net.IPAddress.Parse(TextBox2.Text)
            End If
        End Get
        Set(value As IPAddress)
            TextBox2.Text = value.ToString
        End Set
    End Property
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
    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        sender.Enabled = False
        StartListening()
    End Sub 'start listening button
    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        sender.Enabled = False
        StopListening()
    End Sub 'stop listening button
    Private Function StartListening()
        tcpReceiver = New tcpReceiver_v1(MyListeningIP, MaskedTextBox2.Text, 500, AddressOf DataReceived)
        Select Case tcpReceiver.StartListening
            Case 0
                'timer_Client.Start()
                Return 0
            Case -1
                MsgBox("The selected IP is not assigned to this computer.")
                Return -1
            Case -2
                MsgBox("General error trying to listen.")
                Return -2
        End Select
        Return -900
    End Function
    Private Sub StopListening()
        tcpReceiver.StopListening()
    End Sub
    Public Sub DataReceived(ByVal _data() As String)
        TextBox6.AppendText("<< Someone" & " Says:  " & _data(2) & Newline)
    End Sub 'Data Received
    Private ReadOnly Property ClientIsListening As Boolean
        Get
            If tcpReceiver Is Nothing Then
                Return False
            Else
                Return tcpReceiver.ClientIsListening
            End If
        End Get
    End Property

    'SERVER
    Private Sub SendData_String(ByVal _data As String)
        tcpSender = New tcpSender_v1(TextBox1.Text, MaskedTextBox1.Text)
        tcpSender.SendData(_data)
    End Sub ' Send Data ' String
    Private Function SendChatMessage(ByRef _chatMessage As String, Optional _clearData As Boolean = False)
        If ChatThrottleTimer > -1 Then Return -1
        ChatThrottleTimer = 0
        Dim lasticon = NotifyIcon1.Icon
        NotifyIcon1.Icon = My.Resources.Yellow_Light_Icon_png
        SendData_String(_chatMessage)
        NotifyIcon1.Icon = lasticon
        TextBox6.AppendText(">> You" & " Say:  " & _chatMessage & Newline)
        If _clearData Then
            _chatMessage = ""
        End If
        Return 0
    End Function 'Send Chat Message
    Private Sub TextBox5_KeyDown(sender As System.Object, e As System.Windows.Forms.KeyEventArgs) Handles TextBox5.KeyDown
        If e.KeyCode = Keys.Enter Then
            SendChatMessage(TextBox5.Text, True)
        End If
    End Sub 'send message 'enter pressed
    Private Sub Button3_Click(sender As System.Object, e As System.EventArgs) Handles Button3.Click
        SendChatMessage(TextBox5.Text, True)
    End Sub 'send message 'button pressed

    'UI
    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Me.Location = New Point(250, 100)
        GroupBox1.Enabled = False
        GroupBox2.Enabled = False
        GroupBox3.Enabled = False
        Button1.Enabled = False
        Button2.Enabled = False
        NotifyIcon1.Icon = My.Resources.White_Light_Icon_png

        Button16.PerformClick()
    End Sub 'Form Load
    Private Sub Button16_Click(sender As System.Object, e As System.EventArgs) Handles Button16.Click
        sender.Enabled = False
        TextBox1.Text = DefaultSendToIP 'outbound
        MaskedTextBox1.Text = DefaultPorts 'outbound
        TextBox4.Text = "poop"
        TextBox2.Text = MyIPv4.ToString() 'inbound 'ipv4
        'TextBox2.Text = MyIPv6.ToString() 'inbound 'ipv6
        MaskedTextBox2.Text = DefaultPorts 'inbound
        TextBox3.Text = "poop"
        timer_UI.Start()

        Label10.Text = "/" & TextBox5.MaxLength

        GroupBox1.Enabled = True
        GroupBox2.Enabled = True
        GroupBox3.Enabled = True
    End Sub 'RGC Button
    Private Sub timer_UI_Tick(sender As System.Object, e As System.EventArgs) Handles timer_UI.Tick
        If LastClientIsListeningStatus <> ClientIsListening Then 'if listening change occured..
            LastClientIsListeningStatus = ClientIsListening
            If LastClientIsListeningStatus Then
                Label12.Text = "Listening" 'status label
                Button1.Enabled = False 'start listening
                Button2.Enabled = True 'stop listening button
                TextBox2.Enabled = False
                MaskedTextBox2.Enabled = False
                NotifyIcon1.Icon = My.Resources.Green_Light_Icon_png
            Else
                Label12.Text = "Not Listening" 'status label
                Button1.Enabled = True 'start listening
                Button2.Enabled = False 'stop listening button
                If TextBox2.Text <> "ANY" Then TextBox2.Enabled = True
                MaskedTextBox2.Enabled = True
                NotifyIcon1.Icon = My.Resources.Red_Light_Icon_png
            End If
        End If
        If ChatThrottleTimer > -1 Then
            If ChatThrottleTimer > 500 Then
                ChatThrottleTimer = -1
                Button3.Enabled = True
            Else
                Button3.Enabled = False
                ChatThrottleTimer += sender.Interval
            End If
        End If
    End Sub 'Status / UI Updater 'timer
    Private Sub TextBox5_TextChanged(sender As System.Object, e As System.EventArgs) Handles TextBox5.TextChanged
        'allow right-clicking the label to change to a counting-down mode instead of a counting-up mode.
        Label9.Text = sender.Text.Length
    End Sub 'Updates Used Character Count
    Private Sub Label4_DoubleClick(sender As System.Object, e As System.EventArgs) Handles Label4.DoubleClick
        If ClientIsListening Then Exit Sub
        If TextBox2.Enabled = False Then
            TextBox2.Enabled = True
            TextBox2.Text = MyIPv4.ToString
        Else
            TextBox2.Enabled = False
            TextBox2.Text = "ANY"
        End If
    End Sub 'IP Any / IP Select changer

    'test codes
    Private Sub Label1_DoubleClick(sender As System.Object, e As System.EventArgs) Handles Label1.DoubleClick
        TextBox1.Text = TextBox2.Text
    End Sub
End Class
