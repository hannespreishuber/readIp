' Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

Imports System.Net
Imports Windows.Devices.Enumeration
Imports Windows.Globalization
Imports Windows.Media
Imports Windows.Media.Capture
Imports Windows.Media.MediaProperties
Imports Windows.Media.Ocr
Imports Windows.UI.Popups
''' <summary>
''' Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page
    Private MediaCapture1 As MediaCapture
    Dim dp As New DispatcherTimer
    Private Async Function MainPage_LoadedAsync(sender As Object, e As RoutedEventArgs) As Task Handles Me.Loaded
        dp.Interval = New TimeSpan(0, 0, 0, 0, 100)
        AddHandler dp.Tick, AddressOf ScanIPAsync
        dp.Start()

        If MediaCapture1 Is Nothing Then
            Dim allVideoDevices = Await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture)

            Dim cameraDevice =
            allVideoDevices.FirstOrDefault()
            If cameraDevice Is Nothing Then Return

            MediaCapture1 = New MediaCapture()

            Dim settings = New MediaCaptureInitializationSettings With
            {.VideoDeviceId = cameraDevice.Id}

            Try
                Await MediaCapture1.InitializeAsync(settings)

            Catch ex As Exception
            End Try
            PreviewControl.Source = MediaCapture1
            Await MediaCapture1.StartPreviewAsync()


        End If
    End Function

    Private Async Function ScanIPAsync(sender As Object, e As Object) As Task
        Dim pp As VideoEncodingProperties = MediaCapture1.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview)

        Dim videoFrame = New VideoFrame(Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8,
                                        pp.Width, pp.Height)

        Using currentFrame = Await MediaCapture1.GetPreviewFrameAsync(videoFrame)

            Dim ocrLanguage = New Language("en")
            Dim oe As OcrEngine = OcrEngine.TryCreateFromLanguage(ocrLanguage)

            Dim bitmap = currentFrame.SoftwareBitmap
            Dim imgSource = New WriteableBitmap(bitmap.PixelWidth, bitmap.PixelHeight)
            bitmap.CopyToBuffer(imgSource.PixelBuffer)

            Dim ocrResult = Await oe.RecognizeAsync(bitmap)

            Dim ip = IPAddress.Parse(ocrResult.Text)
            If ip IsNot Nothing Then
                dp.Stop()
                PreviewImage.Source = imgSource

                Dim msg = New MessageDialog("Hab Ip:" + ip.ToString)
                msg.ShowAsync()
            End If
        End Using
    End Function



    'Function ParseIP(data As String) As String
    '    Dim seg(4) As String
    '    Dim segi(4) As Integer
    '    Try
    '        seg = data.Split(".")
    '        For i = 0 To 3
    '            If CInt(seg(i)) >= 0 And seg(i) < 256 Then
    '                segi(i) = CInt(seg(i))
    '            Else
    '                Return Nothing
    '            End If
    '        Next
    '    Catch
    '        Return Nothing
    '    End Try

    '    Return segi(0).ToString + "." + segi(1).ToString + "." + segi(2).ToString + "." + segi(3).ToString
    'End Function

End Class
