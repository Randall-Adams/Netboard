Imports System.IO
Imports System.Net
Imports System.Net.Sockets
Public Class NetboardServer_v1
    Dim DefaultSendToIP As String = ""
    Dim DefaultPorts As Integer = "51649"
    Dim Newline = vbNewLine
    Dim ChatThrottleTimer As Integer = 0
    Dim ChatThrottleTimerIndicator As Integer = 0
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
    Private Function SendChatMessage(ByRef _chatMessage As String, Optional _clearData As Boolean = False, Optional _sendAsServerBoolean As Boolean = False)
        If ChatThrottleTimer > -1 Then Return -1 ' this should be done by tcp class later
        Dim outputbox As TextBox
        If _sendAsServerBoolean Then
            outputbox = TextBox6
            Button3.Enabled = False ' Send Message Button Server
            If ChatThrottleTimerIndicator = 0 Then
                ChatThrottleTimerIndicator = 1
            ElseIf ChatThrottleTimerIndicator = 2 Then
                ChatThrottleTimerIndicator = 3
            End If
        Else
            outputbox = TextBox1
            Button4.Enabled = False ' Send Message Button Client
            If ChatThrottleTimerIndicator = 0 Then
                ChatThrottleTimerIndicator = 2
            ElseIf ChatThrottleTimerIndicator = 1 Then
                ChatThrottleTimerIndicator = 3
            End If
        End If
        ChatThrottleTimer = 0
        Dim lasticon = NotifyIcon1.Icon
        NotifyIcon1.Icon = My.Resources.Yellow_Light_Icon_png
        Kaires.SendChatMessage(_chatMessage, _sendAsServerBoolean)
        NotifyIcon1.Icon = lasticon
        outputbox.AppendText(">> You try to say:  " & _chatMessage & Newline)
        If _clearData Then _chatMessage = ""
        Return 0
    End Function ' Send Chat Message

    'CS - UI
    Private Sub Form1_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load
        Me.Location = New Point(10, 10)
        'Me.Size = New Point(1360, 545)
        Me.Size = New Point(1360, 700)
        NotifyIcon1.Icon = My.Resources.White_Light_Icon_png
        Button7.Enabled = False ' Stop Hosting Button ' button
        GroupBox1.Enabled = False ' Netboard - Main ' Server Page
        GroupBox4.Enabled = False ' Netboard - Groupchat Controls
        TextBox7.Enabled = False ' Display Name ' textbox
        Label13.Enabled = False ' Display Name Label ' label
        TextBox8.Enabled = False ' Passcode ' textbox
        Label14.Enabled = False ' Passcode Label ' label
        MaskedTextBox3.Text = DefaultPorts ' Server Ports ' maskedtextbox
        Label10.Text = "/" & TextBox5.MaxLength ' Your Next Message ' Max Length

        'Server Specific
        TextBox9.Text = MyIPv4.ToString() ' Server IP Address ' textbox

        'Client Boxes
        GroupBox2.Enabled = False ' Netboard - Main 'Client Box
        Label3.Text = "/" & TextBox2.MaxLength ' Your Next Message ' Max Length

        'Commands Box
        ListBox2.Items.Add("SpawnNPC")
        ListBox2.Items.Add("DespawnNPC")
        ListBox2.Items.Add("CreateNPCFile")
        ListBox2.Items.Add("DeleteNPCFile")
        ListBox2.Items.Add("Logoff")
        ListBox2.Items.Add("NPCMessage1")
        CheckCommandValidity()
    End Sub
    Private Sub timer_UI_Tick(sender As System.Object, e As System.EventArgs) Handles timer_UI.Tick
        If 1 = 2 Then
            sender.Stop() ' to turn off timer during debug
        End If
        If LastClientIsListeningStatus <> Kaires.ClientIsListening Then 'if listening change occured..
            LastClientIsListeningStatus = Kaires.ClientIsListening
            If LastClientIsListeningStatus Then
                Label12.Text = "Listening" ' Status Label ' label
                TextBox5.Focus() ' Your Next Message Textbox ' textbox
                NotifyIcon1.Icon = My.Resources.Green_Light_Icon_png
            Else
                Label12.Text = "Not Listening" ' Status Label ' label
                NotifyIcon1.Icon = My.Resources.Red_Light_Icon_png
            End If
        End If
        If ListBox1.Items.Contains(TextBox7.Text) = False Then
            TextBox7.Enabled = True ' Username ' textbox
            Label13.Enabled = True ' Username Label ' label
            TextBox8.Enabled = True ' User Password ' textbox
            Label14.Enabled = True ' User Password Label ' label
            Button1.Enabled = True ' Log On User Account Buttong ' button
            Button2.Enabled = False ' Log Off User Account Buttong ' button

            GroupBox2.Enabled = False
        End If
        If ChatThrottleTimer > -1 Then
            If ChatThrottleTimer > 500 Then
                ChatThrottleTimer = -1
                ChatThrottleTimerIndicator = 0
                Button3.Enabled = True ' Send Message Server ' button
                Button4.Enabled = True ' Send Message Client ' button
            Else
                If ChatThrottleTimerIndicator = 1 Then
                    Button3.Enabled = False ' Send Message Server ' button
                ElseIf ChatThrottleTimerIndicator = 2 Then
                    Button4.Enabled = False ' Send Message Client ' button
                ElseIf ChatThrottleTimerIndicator = 3 Then
                    Button3.Enabled = False ' Send Message Server ' button
                    Button4.Enabled = False ' Send Message Client ' button
                End If
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
            SendChatMessage(TextBox5.Text, True, True)
        End If
    End Sub 'Send Message 'enter pressed
    Private Sub Button3_Click(sender As System.Object, e As System.EventArgs) Handles Button3.Click
        SendChatMessage(TextBox5.Text, True, True)
    End Sub 'Send Message 'button pressed

    'SERVER - CODE
    Private Property MyListeningIP As IPAddress
        Get
            If TextBox9.Text = "ANY" Then
                Return IPAddress.Any
            Else
                Return System.Net.IPAddress.Parse(TextBox9.Text)
            End If
        End Get
        Set(value As IPAddress)
            TextBox9.Text = value.ToString
        End Set
    End Property
    Public Sub DataReceived(ByVal _dataType As String, ByVal _dataObject As Object)
        Select Case _dataType
            Case "console message"
                If IsArray(_dataObject) Then
                    For i As Integer = _dataObject.Length To 1
                        i -= 1
                        TextBox6.AppendText(" >> " & _dataObject(i) & " << " & Newline)
                    Next
                Else
                    TextBox6.AppendText(" >> " & _dataObject & " << " & Newline)
                End If
            Case "connection message"
                Select Case _dataObject(0)
                    Case "logon notify"
                        TextBox6.AppendText(" >> " & _dataObject(1) & " has logged on" & " << " & Newline)
                        ListBox1.Items.Add(_dataObject(1))
                    Case "logoff notify"
                        TextBox6.AppendText(" >> " & _dataObject(1) & " has logged off" & " << " & Newline)
                        ListBox1.Items.Remove(_dataObject(1))
                End Select
            Case "chat message"
                If _dataObject(0) = TextBox7.Text Then
                    TextBox6.AppendText(_dataObject(0) & " says: " & _dataObject(1) & Newline)
                    If GroupBox2.Enabled Then
                        TextBox1.AppendText("You say: " & _dataObject(1) & Newline)
                    End If
                Else
                    TextBox6.AppendText(_dataObject(0) & " says: " & _dataObject(1) & Newline) ' while server show as Server
                    'TextBox6.AppendText("You say: " & _dataObject(1) & Newline) ' while server show as You
                    If GroupBox2.Enabled Then
                        TextBox1.AppendText(_dataObject(0) & " says: " & _dataObject(1) & Newline)
                    End If
                End If
            Case "chat alert"
                If _dataObject(0) = TextBox7.Text Then
                    TextBox1.AppendText(_dataObject(1) & Newline) ' while server show as Server
                Else
                    TextBox1.AppendText(_dataObject(1) & Newline) ' while server show as Server
                End If
            Case "game update"
                If CheckBox1.Checked = False Then
                    TextBox6.AppendText("GU :: " & _dataObject(0) & " IA -> " & _dataObject(1) & ", " & _dataObject(2) & ", " & _dataObject(3) & ", " & _dataObject(4) & Newline)
                End If
            Case "NPCMessage1"
                TextBox6.AppendText("NPC MSG1 :: " & _dataObject(0) & _dataObject(1) & Newline)
            Case "game command"
                TextBox6.AppendText("GCMD :: " & _dataObject(0) & " : " & _dataObject(1) & Newline)
            Case Else
                TextBox6.AppendText(Me.Name & " has received an unknown _dataType in DataReceived()" & Newline)
        End Select
    End Sub 'Data Received

    'SERVER - UI
    Private Sub Button6_Click(sender As System.Object, e As System.EventArgs) Handles Button6.Click
        TextBox7.Enabled = True ' Username ' textbox
        Label13.Enabled = True ' Username Label ' label
        TextBox8.Enabled = True ' User Password ' textbox
        Label14.Enabled = True ' User Password Label ' label
        Button1.Enabled = True ' Log On User Account Buttong ' button
        Button2.Enabled = False ' Log Off User Account Buttong ' button
        GroupBox4.Enabled = True ' Netboard - Groupchat Controls

        MaskedTextBox3.Enabled = False 'outbound port
        GroupBox1.Enabled = True ' Netbpard - Main
        Button6.Enabled = False ' Server Start Hosting ' button
        Button7.Enabled = True ' Server Stop Hosting ' button
        TextBox9.Enabled = False ' Server IP Address

        If Kaires.StartHosting(MyListeningIP, CInt(MaskedTextBox3.Text), 500) = 0 Then
            'successfully hosting ?
        End If
        ' Kaires.ConnectToServer(TextBox7.Text, TextBox8.Text, "127.0.0.1", 51649)

        If Kaires.ClientIsListening = False Then Button7.PerformClick() ' If hosting failed then press stop host button

        timer_UI.Start()
    End Sub ' Start Hosting ' button
    Private Sub Button7_Click(sender As System.Object, e As System.EventArgs) Handles Button7.Click
        Button2.PerformClick()
        Kaires.StopListening()
        MaskedTextBox3.Enabled = True 'outbound port
        GroupBox1.Enabled = False ' Netbpard - Main
        Button7.Enabled = False ' Stop Hosting Button ' button
        Button6.Enabled = True ' Start Hosting ' Button
        If TextBox9.Text <> "ANY" Then TextBox9.Enabled = True

        TextBox7.Enabled = False ' Username ' textbox
        Label13.Enabled = False ' Username Label ' label
        TextBox8.Enabled = False ' User Password ' textbox
        Label14.Enabled = False ' User Password Label ' label
        Button1.Enabled = False ' Log On User Account Buttong ' button
        Button2.Enabled = False ' Log Off User Account Buttong ' button
        GroupBox4.Enabled = False ' Netboard - Groupchat Controls
    End Sub ' Stop Hosting ' button
    Private Sub Label16_Click(sender As System.Object, e As System.EventArgs) Handles Label16.Click
        If Kaires.ClientIsListening Then Exit Sub
        If TextBox9.Enabled = False Then
            TextBox9.Enabled = True
            TextBox9.Text = MyIPv4.ToString
        Else
            TextBox9.Enabled = False
            TextBox9.Text = "ANY"
        End If
    End Sub ' Server IP Changer ' label.Click()

    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs) Handles Button1.Click
        TextBox7.Enabled = False ' Username ' textbox
        Label13.Enabled = False ' Username Label ' label
        TextBox8.Enabled = False ' User Password ' textbox
        Label14.Enabled = False ' User Password Label ' label
        Button1.Enabled = False ' Log On User Account Buttong ' button
        Button2.Enabled = True ' Log Off User Account Buttong ' button
        'TextBox5.Focus()
        Kaires.LogonUser(TextBox7.Text, TextBox8.Text)
        GroupBox2.Enabled = True
    End Sub ' Log On User Account Button ' button

    Private Sub Button2_Click(sender As System.Object, e As System.EventArgs) Handles Button2.Click
        TextBox7.Enabled = True ' Username ' textbox
        Label13.Enabled = True ' Username Label ' label
        TextBox8.Enabled = True ' User Password ' textbox
        Label14.Enabled = True ' User Password Label ' label
        Button1.Enabled = True ' Log On User Account Buttong ' button
        Button2.Enabled = False ' Log Off User Account Buttong ' button

        Kaires.LogoffUser(TextBox7.Text)
        GroupBox2.Enabled = False
    End Sub ' Log Off User Account Button ' button

    '--client stuffs
    Private Sub Button4_Click(sender As System.Object, e As System.EventArgs) Handles Button4.Click
        SendChatMessage(TextBox2.Text, True, False)
    End Sub 'Send Message 'button pressed

    Private Sub TextBox2_KeyDown(sender As System.Object, e As System.Windows.Forms.KeyEventArgs) Handles TextBox2.KeyDown
        If e.KeyCode = Keys.Enter Then
            SendChatMessage(TextBox2.Text, True, False)
        End If
    End Sub

    Private Sub TextBox7_KeyDown(sender As System.Object, e As System.Windows.Forms.KeyEventArgs) Handles TextBox7.KeyDown
        If e.KeyCode = Keys.Enter Then
            Button1.PerformClick()
        End If
    End Sub

    Private Sub TextBox2_TextChanged(sender As System.Object, e As System.EventArgs) Handles TextBox2.TextChanged
        Label4.Text = TextBox2.TextLength
    End Sub

    Private Sub Button5_Click(sender As System.Object, e As System.EventArgs) Handles Button5.Click
        Dim locData(4) As String
        locData(0) = TextBox11.Text
        locData(1) = TextBox3.Text
        locData(2) = TextBox4.Text
        locData(3) = TextBox10.Text
        locData(4) = TextBox12.Text
        TextBox6.AppendText(" Try >> GU :: " & locData(0) & " IA -> " & locData(1) & ", " & locData(2) & ", " & locData(3) & ", " & locData(4) & Newline)
        Kaires.SendGameUpdate(locData, True)
    End Sub

    Private Sub Button8_Click(sender As System.Object, e As System.EventArgs) Handles Button8.Click
        Select Case ListBox2.Text
            Case "SpawnNPC"
                Kaires.SendSpawnNPC(ListBox1.Text, InputBox("NPC to spawn:", "Select the NPC to spawn in the user's world"))
            Case "DespawnNPC"

            Case "CreateNPCFile"

            Case "DeleteNPCFile"

            Case "Logoff"
                Kaires.LogoffUserNotify(ListBox1.Text)
            Case "NPCMessage1"
                Dim res(1) As String
                res(0) = InputBox("NPC to speak:", "Select the NPC to speak to the user")
                res(1) = InputBox("NPC Message:", "Select the NPC message to be said")
                Kaires.SendNPCMessage(ListBox1.Text, res(0), res(1))
        End Select
    End Sub

    Private Sub ListBox1_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles ListBox1.SelectedIndexChanged
        CheckCommandValidity()
    End Sub ' user list

    Private Sub ListBox2_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles ListBox2.SelectedIndexChanged

    End Sub ' command list

    Private Sub CheckCommandValidity() Handles ListBox1.SelectedIndexChanged, ListBox2.SelectedIndexChanged
        If ListBox1.SelectedIndex = -1 Or ListBox2.SelectedIndex = -1 Then
            Button8.Enabled = False
        Else
            Button8.Enabled = True
        End If
    End Sub

    Private Sub Button9_Click(sender As System.Object, e As System.EventArgs) Handles Button9.Click
        If TextBox13.Text = "" Then
            TextBox13.Text = InputBox("Allow which IP to start asking to connect?", "IP Connect Allow")
        End If
        Kaires.SendAllowConnectionRequest(True, TextBox13.Text)
        If ListBox3.Items.Contains(TextBox13.Text) = False Then
            ListBox3.Items.Add(TextBox13.Text)
        End If
    End Sub ' ip connect allow
    Private Sub Button10_Click(sender As System.Object, e As System.EventArgs) Handles Button10.Click
        If TextBox13.Text = "" Then
            TextBox13.Text = InputBox("Disallow which IP to start asking to connect?", "IP Connect Disallow")
        End If
        Kaires.SendAllowConnectionRequest(False, TextBox13.Text)
        If ListBox3.Items.Contains(TextBox13.Text) = False Then
            ListBox3.Items.Add(TextBox13.Text)
        End If
    End Sub ' ip connect disallow

    Private Sub ListBox3_SelectedIndexChanged(sender As System.Object, e As System.EventArgs) Handles ListBox3.SelectedIndexChanged
        If sender.SelectedIndex <> -1 Then
            TextBox13.Text = sender.Text
        End If
    End Sub

    Private Sub Label19_Click(sender As System.Object, e As System.EventArgs) Handles Label19.Click
        ListBox3.SelectedIndex = -1
    End Sub
End Class
