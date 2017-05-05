Imports System.Text.RegularExpressions
Imports System.Security.Principal
Imports System.ServiceProcess

Public Class main
    Dim identity = WindowsIdentity.GetCurrent()
    Dim principal = New WindowsPrincipal(identity)
    Dim isRunAsAdmin As Boolean = principal.IsInRole(WindowsBuiltInRole.Administrator)
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles btnAdd.Click
        Dim msgBoxShown As Boolean = False
        btnXAMPPLocation.Enabled = False
        btnLaravelLocation.Enabled = False
        tbDomainName.Enabled = False
        tbWebPort.Enabled = False
        btnAdd.Enabled = False
        pbWorking.Visible = True
        Try

            If Not IO.Directory.Exists(tbXAMPPLocation.Text) Then
                MsgBox("XAMPP directory not found", MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly)
                msgBoxShown = True
                Exit Try
            End If

            If Not IO.Directory.Exists(tbXAMPPLocation.Text & "\apache") Then
                MsgBox("apache directory not found", MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly)
                msgBoxShown = True
                Exit Try
            End If

            If Not IO.Directory.Exists(tbXAMPPLocation.Text & "\apache\conf") Then
                MsgBox("apache conf directory not found", MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly)
                msgBoxShown = True
                Exit Try
            End If

            If Not IO.Directory.Exists(tbXAMPPLocation.Text & "\apache\conf\extra") Then
                MsgBox("apache conf extra directory not found", MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly)
                msgBoxShown = True
                Exit Try
            End If

            If Not IO.File.Exists(tbXAMPPLocation.Text & "\apache\conf\extra\httpd-vhosts.conf") Then
                MsgBox("httpd-vhosts.conf not found", MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly)
                msgBoxShown = True
                Exit Try
            End If

            If tbDomainName.Text = Nothing Then
                MsgBox("You must define a domain name", MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly)
                msgBoxShown = True
                Exit Try
            End If

            If Not Regex.IsMatch(tbDomainName.Text, "^[a-z0-9]+$") Then
                MsgBox("You must define a domain name with lowercase letters and numbers", MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly)
                msgBoxShown = True
                Exit Try
            End If

            If tbLaravelLocation.Text = Nothing Then
                MsgBox("You must define the laravel public folder location", MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly)
                msgBoxShown = True
                Exit Try
            End If

            If tbLaravelLocation.Text.EndsWith("/public") = False Then
                MsgBox("You must define the laravel public folder location", MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly)
                msgBoxShown = True
                Exit Try
            End If

            If tbWebPort.Text = Nothing Then
                MsgBox("You must define the laravel public folder location", MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly)
                msgBoxShown = True
                Exit Try
            End If

            If Not Regex.IsMatch(tbWebPort.Text, "^[0-9]+$") Then
                MsgBox("You must define a port numbers", MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly)
                msgBoxShown = True
                Exit Try
            End If

            If Not IO.File.Exists("C:\Windows\System32\drivers\etc\hosts") Then
                MsgBox("hosts file not found", MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly)
                msgBoxShown = True
                Exit Try
            End If

            IO.File.Copy("C:\Windows\System32\drivers\etc\hosts", My.Computer.FileSystem.SpecialDirectories.Desktop & "\hosts", True)

            Using sw As IO.StreamWriter = IO.File.AppendText(My.Computer.FileSystem.SpecialDirectories.Desktop & "\hosts")
                sw.WriteLine("")
                sw.WriteLine("127.0.0.1 " & tbDomainName.Text & ".dev")
            End Using

            IO.File.Copy(My.Computer.FileSystem.SpecialDirectories.Desktop & "\hosts", "C:\Windows\System32\drivers\etc\hosts", True)
            IO.File.Delete(My.Computer.FileSystem.SpecialDirectories.Desktop & "\hosts")

            Using sw As IO.StreamWriter = IO.File.AppendText(tbXAMPPLocation.Text & "\apache\conf\extra\httpd-vhosts.conf")
                sw.WriteLine("")
                sw.WriteLine("")
                sw.WriteLine("<VirtualHost " & tbDomainName.Text & ".dev:" & tbWebPort.Text & ">")
                sw.WriteLine("  DocumentRoot """ & tbLaravelLocation.Text & """")
                sw.WriteLine("  ServerName " & tbDomainName.Text & ".dev")
                sw.WriteLine("  <Directory """ & tbLaravelLocation.Text & """>")
                sw.WriteLine("      Require all granted")
                sw.WriteLine("      AllowOverride All")
                sw.WriteLine("  </Directory>")
                sw.WriteLine("</VirtualHost>")
            End Using

            Dim service As ServiceController = New ServiceController("Apache2.4")
            If ((service.Status.Equals(ServiceControllerStatus.Stopped)) Or (service.Status.Equals(ServiceControllerStatus.StopPending))) Then
                service.Start()
            Else
                service.Stop()
                service.WaitForStatus(ServiceControllerStatus.Stopped)
                service.Start()
            End If

        Catch ex As Exception
            MsgBox(ex.Message & vbNewLine & vbNewLine & ex.StackTrace, MsgBoxStyle.Critical Or MsgBoxStyle.OkOnly)
        Finally
            btnXAMPPLocation.Enabled = True
            btnLaravelLocation.Enabled = True
            tbDomainName.Enabled = True
            tbWebPort.Enabled = True
            btnAdd.Enabled = True
            pbWorking.Visible = False
            tbLaravelLocation.Text = ""
            tbDomainName.Text = ""
            tbWebPort.Text = ""
            tbLaravelLocation.Text = "C:\xampp"
            If msgBoxShown = False Then
                MsgBox("Files has been written. And Apache service has been restarted!", MsgBoxStyle.Information Or MsgBoxStyle.OkOnly)
            End If
        End Try
    End Sub

    Private Sub Button1_Click_1(sender As Object, e As EventArgs) Handles btnLaravelLocation.Click
        Dim dialog As New FolderBrowserDialog()
        dialog.RootFolder = Environment.SpecialFolder.MyComputer
        If dialog.ShowDialog() = Windows.Forms.DialogResult.OK Then
            Dim selPath As String = dialog.SelectedPath
            tbLaravelLocation.Text = selPath.Replace("\", "/").ToString
        End If
    End Sub

    Private Sub main_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If isRunAsAdmin = False Then
            MsgBox("This application needs to be started as an administrator to get access to do all the changes to files and services.", MsgBoxStyle.Exclamation Or MsgBoxStyle.OkOnly)
            tsslAdmin.Text = "Start application as admin!"
            tsslAdmin.IsLink = True
        Else
            tsslAdmin.Text = "Running as admin"
            tsslAdmin.ForeColor = Color.OrangeRed
            tsslAdmin.Font = New Font("Segoe UI", 9, FontStyle.Italic)
        End If
        tsslVersion.Text = My.Application.Info.Version.Major.ToString & "." & My.Application.Info.Version.Minor.ToString & "." & My.Application.Info.Version.Build.ToString & " Build " & My.Application.Info.Version.Revision.ToString
    End Sub

    Private Sub btnXAMPPLocation_Click(sender As Object, e As EventArgs) Handles btnXAMPPLocation.Click
        Dim dialog As New FolderBrowserDialog()
        dialog.RootFolder = Environment.SpecialFolder.MyComputer
        If dialog.ShowDialog() = Windows.Forms.DialogResult.OK Then
            Dim selPath As String = dialog.SelectedPath
            tbXAMPPLocation.Text = selPath
        End If
    End Sub

    Private Sub ToolStripStatusLabel3_Click(sender As Object, e As EventArgs) Handles ToolStripStatusLabel3.Click
        Process.Start("https://www.infihex.com/")
    End Sub

    Private Sub tsslAdmin_Click(sender As Object, e As EventArgs) Handles tsslAdmin.Click
        If isRunAsAdmin = False Then
            Application.Exit()
            'Process.Start(Application.StartupPath)
            Dim process As New Process()
            process.StartInfo.FileName = Application.ExecutablePath
            process.StartInfo.Verb = "runas"
            process.StartInfo.UseShellExecute = True
            process.Start()
        End If
    End Sub
End Class
