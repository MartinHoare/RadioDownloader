<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Status
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.lblStatus = New System.Windows.Forms.Label
        Me.prgProgress = New System.Windows.Forms.ProgressBar
        Me.SuspendLayout()
        '
        'lblStatus
        '
        Me.lblStatus.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStatus.Location = New System.Drawing.Point(12, 9)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(414, 86)
        Me.lblStatus.TabIndex = 0
        Me.lblStatus.Text = "Please wait"
        Me.lblStatus.TextAlign = System.Drawing.ContentAlignment.BottomCenter
        '
        'prgProgress
        '
        Me.prgProgress.Location = New System.Drawing.Point(15, 110)
        Me.prgProgress.Name = "prgProgress"
        Me.prgProgress.Size = New System.Drawing.Size(411, 23)
        Me.prgProgress.TabIndex = 1
        '
        'Status
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(96.0!, 96.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi
        Me.ClientSize = New System.Drawing.Size(438, 250)
        Me.ControlBox = False
        Me.Controls.Add(Me.prgProgress)
        Me.Controls.Add(Me.lblStatus)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = Global.RadioDld.My.Resources.Resources.icon_main
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "Status"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Radio Downloader"
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents lblStatus As System.Windows.Forms.Label
    Private WithEvents prgProgress As System.Windows.Forms.ProgressBar
End Class
