﻿Friend NotInheritable Class Bootstrap
    Private Sub New()
    End Sub

    Public Shared Sub Main()
        IO.Directory.SetCurrentDirectory(IO.Path.GetDirectoryName(Reflection.Assembly.GetEntryAssembly().Location))
        If VerifyLocalDependanicesExist() Then Program.Run()
    End Sub

    Private Shared Function VerifyLocalDependanicesExist() As Boolean
        Try
            Reflection.Assembly.ReflectionOnlyLoad("Desktop Sprites")
        Catch ex As Exception
            Dim message =
                "Some required files are missing! " &
                "If you have just downloaded Desktop Ponies please ensure you have extracted everything. " &
                "If you are trying to run Desktop Ponies without extracting first it won't work!"
            Console.WriteLine(message)
            Dim form = New Form() With {.StartPosition = FormStartPosition.CenterScreen, .Size = Size.Empty, .Text = "Fatal Error"}
            AddHandler form.Shown,
                Sub()
                    MessageBox.Show(form, message, "Required Files Missing", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    form.Close()
                End Sub
            Application.Run(form)
            Return False
        End Try
        Return True
    End Function
End Class

Friend NotInheritable Class Program
    Private Sub New()
    End Sub

    Public Shared Sub Run()
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf AppDomain_UnhandledException
        AddHandler Threading.Tasks.TaskScheduler.UnobservedTaskException, AddressOf TaskScheduler_UnobservedTaskException
        If Not OperatingSystemInfo.IsMacOSX Then
            RunWinForms()
        Else
            RunGtk()
        End If
    End Sub

    Private Shared Sub RunWinForms()
        AddHandler Application.ThreadException, AddressOf Application_ThreadException
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.Run(New MainForm())
    End Sub

    Private Shared Sub RunGtk()
        Gtk.Application.Init()
        Dim window = New MainWindow()
        AddHandler window.DeleteEvent, Sub() Gtk.Application.Quit()
        window.ShowAll()
        Gtk.Application.Run()
    End Sub

    Private Shared Sub AppDomain_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        NotifyUserOfFatalExceptionAndExit(DirectCast(e.ExceptionObject, Exception))
    End Sub

    Private Shared Sub TaskScheduler_UnobservedTaskException(sender As Object, e As Threading.Tasks.UnobservedTaskExceptionEventArgs)
        ' If a debugger is attached, this event is not raised (instead the exception is allowed to propagate to the debugger),
        ' therefore we'll just log since ending the process gains no additional safety at this point.
        e.SetObserved()
        LogErrorToConsole(e.Exception, "Unobserved Task Exception")
        LogErrorToDisk(e.Exception)
    End Sub

    Private Shared Sub Application_ThreadException(sender As Object, e As Threading.ThreadExceptionEventArgs)
        NotifyUserOfFatalExceptionAndExit(e.Exception)
    End Sub

    Public Shared Sub NotifyUserOfNonFatalException(ex As Exception, message As String)
        LogErrorToConsole(ex, "WARNING: " & message)
        If Not OperatingSystemInfo.IsMacOSX Then
            ExceptionDialog.Show(ex, message, "Warning - Desktop Ponies v" & General.GetAssemblyVersion().ToDisplayString(), False)
        End If
    End Sub

    Public Shared Sub NotifyUserOfFatalExceptionAndExit(ex As Exception)
        Try
            ' Attempt to log error.
            Try
                LogErrorToConsole(ex, "FATAL: An unexpected error occurred and Desktop Ponies must close.")
                LogErrorToDisk(ex)
            Catch
                ' Logging might fail, but we'll just have to live with that.
                Console.WriteLine("An unexpected error occurred and Desktop Ponies must close. (An error file could not be generated)")
            End Try

            If Not OperatingSystemInfo.IsMacOSX Then
                ' Attempt to notify user of an unknown error.
                ExceptionDialog.Show(ex, "An unexpected error occurred and Desktop Ponies must close." &
                                     " Please report this error so it can be fixed.",
                                     "Unexpected Error - Desktop Ponies v" & General.GetAssemblyVersion().ToDisplayString(), True)
            End If
        Catch
            ' The application is already in an unreliable state, we're just trying to exit as cleanly as possible now.
        Finally
            ' Exit the program with an error code, unless a debugger is attached in which case we'll let the exception bubble to the
            ' debugger for analysis.
            If Not Diagnostics.Debugger.IsAttached Then Environment.Exit(1)
        End Try
    End Sub

    Private Shared Sub LogErrorToConsole(ex As Exception, message As String)
        Console.WriteLine("-----")
        Console.WriteLine(message)
        Console.WriteLine(
            "Error in Desktop Ponies v" & General.GetAssemblyVersion().ToDisplayString() & " occurred " &
            Date.UtcNow.ToString("u", Globalization.CultureInfo.InvariantCulture))
        Console.WriteLine()
        Console.WriteLine(ex.ToString())
        Console.WriteLine("-----")
    End Sub

    Private Shared Sub LogErrorToDisk(ex As Exception)
        Const path = "error.txt"
        Using errorFile As New IO.StreamWriter(path, False, Text.Encoding.UTF8)
            errorFile.WriteLine(
                "Unhandled error in Desktop Ponies v" & General.GetAssemblyVersion().ToDisplayString() &
                " occurred " & Date.UtcNow.ToString("u", Globalization.CultureInfo.InvariantCulture))
            errorFile.WriteLine()
            errorFile.WriteLine(ex.ToString())
            Console.WriteLine("An error file can be found at " & path)
        End Using
    End Sub
End Class
