Imports System.Net

Public Class ZCD
    Public version As String = "v0.4.0 "

    Dim tags As String = ""
    Dim globalstring As String = ""
    Dim state As Boolean = False
    Dim progress As Int32 = 0
    Dim pages As Int16
    Dim modifier As String


    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        modifier = ""
        tags = TextBox1.Text
        pages = NumericUpDown1.Value

        MakeModifier(modifier)

        If BackgroundWorker1.IsBusy Then
            MsgBox("Hey, Don't rush me ;_; , I'm busy")
        Else
            BackgroundWorker1.RunWorkerAsync()
            ' The connection and source grabbing is very slow so I'd rather have
            ' it run in the background so the GUI doesn't freeze for a long time
        End If
        Me.UseWaitCursor = True
        'TextBox2.Text = ThumbToFull(TextBox2.Text)
        Me.UseWaitCursor = False
    End Sub

    Function GetSource(ByVal s As String) As String
        s = s.Replace(" ", "+")
        Dim request As HttpWebRequest = Nothing
        Dim response As HttpWebResponse = Nothing

        Try
            request = HttpWebRequest.Create("http://www.zerochan.net/" + s)
            request.AllowWriteStreamBuffering = True
        Catch ex As Exception
            MsgBox("Connection to ZeroChan didn't work. That happens sometimes, dunno why, try again.")
            Return " "
        End Try

        Try
            response = request.GetResponse()
        Catch ex As Exception
            MsgBox("Cannot work with that http response, try again." & vbNewLine & "Maybe you wrote the wrong tags or something.")
        End Try

        Dim sr As IO.StreamReader = New IO.StreamReader(response.GetResponseStream())
        Dim sourcecode As String = sr.ReadToEnd

        response.Close()

        Return sourcecode
    End Function

    Function GetLinks(ByVal s As String) As String
        Dim pos As Integer
        Dim temp As String = ""
        Dim res As String = ""


        pos = s.IndexOf("http://")

        While pos > 0


            If pos > s.Length Then
                Exit While
            End If

            s = s.Remove(0, pos)

            pos = s.IndexOf(".jpg")

            temp = s.Substring(0, pos + 4)

            If temp Like "http://*.zerochan.net/*/*.jpg" Then
                If pos < 60 Then
                    temp = temp.Replace("/240/", "/full/")
                    res += temp & vbNewLine
                End If
            End If

            s = s.Remove(0, 2)
            pos = s.IndexOf("http://")

        End While


        Return res
    End Function

    Function ThumbToFull(ByVal s As String) As String
        Return s.Replace("240/", "full/")
    End Function

    Function MakeModifier(ByRef m As String)
        If Strict.Checked Then
            m = "?strict&p="
        Else
            m = "?p="
        End If
        Return m
    End Function

    Function SaveToDisk(ByVal s As String)
        Dim img As Image = Nothing
        Dim links() As String

        FileIO.FileSystem.CreateDirectory(LocationBar.Text)

        links = s.Split(vbNewLine)

        For i = 0 To links.Length - 1
            progress = (i / (links.Length - 1)) * 100
            If links.ElementAt(i).Equals("") Then
            Else
                img = DownloadImage(links.ElementAt(i))
            End If
            If IsNothing(img) Then
            Else
                img.Save(LocationBar.Text & "\\" & links.ElementAt(i).Substring(links.ElementAt(i).LastIndexOf("/")))
            End If
        Next i
        MsgBox("Finished downloading. Or at least I tried. See log.txt for links to" & vbNewLine & "files that weren't downloaded.")
        Return True
    End Function

    Function DownloadImage(ByVal link As String) As Image
        Dim tmpImage As Image = Nothing

        Try
            Dim Request As System.Net.HttpWebRequest = CType(System.Net.HttpWebRequest.Create(link), System.Net.HttpWebRequest)

            Request.AllowWriteStreamBuffering = True
            Dim Response As System.Net.WebResponse = Request.GetResponse()
            Dim WebStr As System.IO.Stream = Response.GetResponseStream()
            tmpImage = Image.FromStream(WebStr)
            Response.Close()
        Catch Exception As Exception
            If link.Length > 1 Then
                MsgBox("Error downloading: " & link)
                Dim filelog As New System.IO.FileStream(LocationBar.Text & "\log.txt", IO.FileMode.Append)
                Dim sw As New System.IO.StreamWriter(filelog)
                sw.WriteLine(System.DateTime.Now.ToString() & " - " & link & " was not downloaded.")
            End If
            Return Nothing
        End Try

        Return tmpImage
    End Function

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button2.Click
        If BackgroundWorker2.IsBusy Then
            MsgBox("Already doing stuff here...")
        Else
            BackgroundWorker2.RunWorkerAsync()
            ' Again a time consuming part of the code
        End If
    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TextUpdate.Tick
        ProgressBar1.Visible = state
        ProgressBar1.Value = progress
        TextBox2.Text = globalstring
    End Sub

    Private Sub BackgroundWorker1_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker1.DoWork
        state = True
        Dim localstring As String
        For i = 1 To pages
            localstring = GetSource(tags & modifier & i)
            progress = i / pages * 50
            globalstring += GetLinks(localstring)
        Next i
        state = False
        progress = 0
    End Sub

    Private Sub BackgroundWorker2_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackgroundWorker2.DoWork
        state = True
        If String.IsNullOrEmpty(globalstring) Then
            MsgBox("Did you forget to hit the GET LINKS button?")
        Else
            SaveToDisk(globalstring)
        End If
        state = False
    End Sub

    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click
        Me.Close()
    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        Dialog1.ShowDialog()
    End Sub

    Private Sub AbortButton_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AbortButton.Click
        BackgroundWorker1.CancelAsync()
        progress = 0
        state = False
    End Sub

    Private Sub Button3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button3.Click
        FolderBrowserDialog1.ShowDialog()
        LocationBar.Text = FolderBrowserDialog1.SelectedPath
    End Sub
End Class