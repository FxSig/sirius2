﻿/*
 *                                                            ,--,      ,--,                              
 *             ,-.----.                                     ,---.'|   ,---.'|                              
 *   .--.--.   \    /  \     ,---,,-.----.      ,---,       |   | :   |   | :      ,---,           ,---,.  
 *  /  /    '. |   :    \ ,`--.' |\    /  \    '  .' \      :   : |   :   : |     '  .' \        ,'  .'  \ 
 * |  :  /`. / |   |  .\ :|   :  :;   :    \  /  ;    '.    |   ' :   |   ' :    /  ;    '.    ,---.' .' | 
 * ;  |  |--`  .   :  |: |:   |  '|   | .\ : :  :       \   ;   ; '   ;   ; '   :  :       \   |   |  |: | 
 * |  :  ;_    |   |   \ :|   :  |.   : |: | :  |   /\   \  '   | |__ '   | |__ :  |   /\   \  :   :  :  / 
 *  \  \    `. |   : .   /'   '  ;|   |  \ : |  :  ' ;.   : |   | :.'||   | :.'||  :  ' ;.   : :   |    ;  
 *   `----.   \;   | |`-' |   |  ||   : .  / |  |  ;/  \   \'   :    ;'   :    ;|  |  ;/  \   \|   :     \ 
 *   __ \  \  ||   | ;    '   :  ;;   | |  \ '  :  | \  \ ,'|   |  ./ |   |  ./ '  :  | \  \ ,'|   |   . | 
 *  /  /`--'  /:   ' |    |   |  '|   | ;\  \|  |  '  '--'  ;   : ;   ;   : ;   |  |  '  '--'  '   :  '; | 
 * '--'.     / :   : :    '   :  |:   ' | \.'|  :  :        |   ,/    |   ,/    |  :  :        |   |  | ;  
 *   `--'---'  |   | :    ;   |.' :   : :-'  |  | ,'        '---'     '---'     |  | ,'        |   :   /   
 *             `---'.|    '---'   |   |.'    `--''                              `--''          |   | ,'    
 *               `---`            `---'                                                        `----'   
 * 
 * 2023 Copyright to (c)SpiralLAB. All rights reserved.
 * Description : SiriusEditor usercontrol
 * Author : hong chan, choi / hcchoi@spirallab.co.kr (http://spirallab.co.kr)
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;
using System.Runtime.InteropServices;
using SpiralLab.Sirius2.Winforms;
using SpiralLab.Sirius2.Winforms.UI;
using SpiralLab.Sirius2.IO;
using SpiralLab.Sirius2.Laser;
using SpiralLab.Sirius2.Scanner.Rtc;
using SpiralLab.Sirius2.Scanner.Rtc.SyncAxis;
using SpiralLab.Sirius2.Winforms.Entity;
using SpiralLab.Sirius2.Winforms.Marker;
using SpiralLab.Sirius2.Winforms.Common;
using SpiralLab.Sirius2.PowerMeter;
using SpiralLab.Sirius2.PowerMap;
using OpenTK;

namespace Demos
{
    /// <summary>
    /// SiriusEditorUserControl
    /// </summary>
    /// <remarks>
    /// User can insert(or create) usercontrol at own winforms. <br/>
    /// </remarks>
    public partial class SiriusEditorUserControl : Form
    {
        /// <summary>
        /// Title name
        /// </summary>
        public string TitleName
        {
            get { return lblName.Text; }
            set { lblName.Text = value; }
        }

        /// <summary>
        /// Disable UI controls or not
        /// </summary>
        /// <remarks>
        /// To do not allow user operations. <br/>
        /// </remarks>
        public bool IsDisableControl
        {
            get { return isDisableControl; }
            set
            {
                isDisableControl = value;
                if (isDisableControl)
                {
                    TreeViewCtrl.Enabled = false;
                    TreeViewBlockCtrl.Enabled = false;
                    PropertyGridCtrl.Enabled = false;
                    EditorCtrl.Enabled = false;
                    //PenCtrl.Enabled = false;
                    LaserCtrl.Enabled = false;
                    //MarkerCtrl.Enabled = false;
                    OffsetCtrl.Enabled = false;
                    RtcDICtrl.Enabled = false;
                    RtcDOCtrl.Enabled = false;
                    ManualCtrl.Enabled = false;
                }
                else
                {
                    TreeViewCtrl.Enabled = true;
                    TreeViewBlockCtrl.Enabled = true;
                    PropertyGridCtrl.Enabled = true;
                    EditorCtrl.Enabled = true;
                    //PenCtrl.Enabled = false;
                    LaserCtrl.Enabled = true;
                    //MarkerCtrl.Enabled = true;
                    OffsetCtrl.Enabled = true;
                    RtcDICtrl.Enabled = true;
                    RtcDOCtrl.Enabled = true;
                    ManualCtrl.Enabled = true;
                }
            }
        }
        private bool isDisableControl;

        /// <summary>
        /// <c>IDocument</c>
        /// </summary>
        /// <remarks>
        /// Created by automatically <br/>
        /// </remarks>
        public IDocument Document
        {
            get { return document; }
            protected set 
            {
                if (document == value)
                    return;
                if (document != null)
                {
                    document.OnSelected -= Document_OnSelected;
                    document.OnSaved -= Document_OnSaved;
                    document.OnOpened -= Document_OnOpened;
                    if (EditorCtrl.View is ViewBase vb)
                    {
                        vb.Renderer.MouseMove -= Renderer_MouseMove;
                        vb.Renderer.Paint -= Renderer_Paint;
                    }
                }
                document = value;
                PropertyGridCtrl.Document = document;
                PenCtrl.Document = document;
                TreeViewCtrl.Document = document;
                TreeViewBlockCtrl.Document = document;
                EditorCtrl.Document = document;
                //RtcControl
                //LaserCOntrol
                if (document != null)
                {
                    document.OnSelected += Document_OnSelected;
                    document.OnSaved += Document_OnSaved;
                    document.OnOpened += Document_OnOpened;
                    if (EditorCtrl.View is ViewBase vb)
                    {
                        vb.Renderer.MouseMove += Renderer_MouseMove;
                        vb.Renderer.Paint += Renderer_Paint;
                    }
                }
                PropertyGridCtrl.SelecteObject = null;
            }
        }  
        private IDocument document;

        /// <summary>
        /// <c>IRtc</c>
        /// </summary>
        /// <remarks>
        /// Created by <c>ScannerFactory</c> <br/>
        /// </remarks>
        public IRtc Rtc
        {
            get { return rtc; }
            set
            {
                if (rtc == value)
                    return;                
                if (rtc != null)
                {
                    if (rtc is IRtcMoF mof)
                    {
                        mof.OnEncoderChanged -= Mof_OnEncoderChanged;
                    }
                    myDIExt1?.Dispose();
                    myDOExt1?.Dispose();
                    myDOExt2?.Dispose();
                    myDILaserPort?.Dispose();
                    myDOLaserPort?.Dispose();

                    myDIExt1 = null;
                    myDILaserPort = null;
                    myDOExt1 = null;
                    myDOExt2 = null;
                    myDOLaserPort = null;
                }
                
                rtc = value;
                RtcCtrl.Rtc = rtc;
                RtcDICtrl.Rtc = rtc;
                RtcDOCtrl.Rtc = rtc;
                ManualCtrl.Rtc = rtc;
                EditorCtrl.Rtc = rtc;
                TreeViewCtrl.Rtc = rtc;
                TreeViewBlockCtrl.Rtc = rtc;
                PowerMapCtrl.Rtc = rtc;

                if (rtc != null)
                {
                    // RTC extension DIO
                    myDIExt1 = IOFactory.CreateInputExtension1(rtc);
                    myDOExt1 = IOFactory.CreateOutputExtension1(rtc);
                    if (rtc is IRtcSyncAxis)
                    {
                    }
                    else
                    {
                        myDILaserPort = IOFactory.CreateInputLaserPort(rtc);
                        myDOLaserPort = IOFactory.CreateOutputLaserPort(rtc);
                        myDOExt2 = IOFactory.CreateOutputExtension2(rtc);
                    }

                    myDIExt1?.Initialize();
                    myDOExt1?.Initialize();
                    myDOExt2?.Initialize();
                    myDILaserPort?.Initialize();
                    myDOLaserPort?.Initialize();

                    rtcDIUserControl1.DIExt1 = myDIExt1;
                    rtcDIUserControl1.DILaserPort = myDILaserPort;
                    rtcDIUserControl1.UpdateExtension1PortNames(Config.DIN_RtcExtension1Port);
                    rtcDIUserControl1.UpdateLaserPortNames(Config.DIN_RtcLaserPort);
                    
                    rtcDOUserControl1.DOExt1 = myDOExt1;
                    rtcDOUserControl1.DOExt2 = myDOExt2;
                    rtcDOUserControl1.DOLaserPort = myDOLaserPort;
                    rtcDOUserControl1.UpdateExtension1PortNames(Config.DOut_RtcExtension1Port);
                    rtcDOUserControl1.UpdateExtension2PortNames(Config.DOut_RtcExtension2Port);
                    rtcDOUserControl1.UpdateLaserPortNames(Config.DOut_RtcLaserPort);

                    EntityVisibility();
                    MenuVisibility();
                    if (rtc is IRtcMoF mof)
                    {
                        mof.OnEncoderChanged += Mof_OnEncoderChanged;
                    }
                }
            }
        }
        private IRtc rtc;

        /// <summary>
        /// <c>ILaser</c>
        /// </summary>
        /// <remarks>
        /// Created by <c>LaserFactory</c> <br/>
        /// </remarks>
        public ILaser Laser
        {
            get { return laser; }
            set
            {
                if (laser == value)
                    return;
                laser = value;
                LaserCtrl.Laser = laser;
                ManualCtrl.Laser = laser;
                PowerMeterCtrl.Laser = laser;
                PowerMapCtrl.Laser = laser;
                if (null != laser)
                {
                    EntityPen.PropertyVisibility(laser);
                    var powerControl = laser as ILaserPowerControl;
                    foreach (var pen in document.InternalData.Pens)
                    {
                        pen.PowerMax = laser.MaxPowerWatt;
                        if (null != powerControl)
                            pen.PowerMap = powerControl.PowerMap;
                        else
                            pen.PowerMap = null;
                    }
                }
            }
        }
        private ILaser laser;

        /// <summary>
        /// <c>IMarker</c>
        /// </summary>
        /// <remarks>
        /// Created by <c>MarkerFactory</c> <br/>
        /// </remarks>
        public IMarker Marker
        {
            get { return marker; }
            set
            {
                if (marker == value)
                    return;
                if (marker != null)
                {
                    marker.OnStarted -= Marker_OnStarted;
                    marker.OnEnded -= Marker_OnEnded;
                }
                marker = value;
                markerControl1.Marker = marker;
                offsetControl1.Marker = marker;
                remoteUserControl1.Marker = marker;
                EditorCtrl.View.Marker = marker;
                //marker browsable
                if (marker != null)
                {
                    marker.OnStarted += Marker_OnStarted;
                    marker.OnEnded += Marker_OnEnded;
                }
            }
        }
        private IMarker marker;

        /// <summary>
        /// <c>IView</c>
        /// </summary>
        /// <remarks>
        /// Created by automatically <br/>
        /// </remarks>
        public IView View
        {
            get { return EditorCtrl.View; }
        }

        /// <summary>
        /// <c>IRemote</c>
        /// </summary>
        /// <remarks>
        /// Created by <c>RemoteFactory</c> <br/>
        /// </remarks>
        public IRemote Remote 
        {
            get { return remote; }
            set
            {
                if (remote == value)
                    return;
                if (remote != null)
                {
                }

                remote = value;
                remoteUserControl1.Remote = remote;
                if (marker != null)
                {
                }
            }
        }
        private IRemote remote;

        /// <summary>
        /// <c>IPowerMeter</c>
        /// </summary>
        /// <remarks>
        /// Created by <c>PowerMeterFactory</c> <br/>
        /// </remarks>
        public IPowerMeter PowerMeter
        {
            get { return powerMeter; }
            set
            {
                if (powerMeter == value)
                    return;
                if (powerMeter != null)
                {
                    powerMeter.OnStarted -= PowerMeter_OnStarted;
                    powerMeter.OnStopped -= PowerMeter_OnStopped;
                    powerMeter.OnMeasured -= PowerMeter_OnMeasured;
                    powerMeter.OnCleared -= PowerMeter_OnCleared;
                }
                powerMeter = value;
                PowerMeterCtrl.PowerMeter = powerMeter;
                PowerMapCtrl.PowerMeter = powerMeter;
                if (powerMeter != null)
                {
                    lblPowerWatt.Text = "0.0 W";
                    powerMeter.OnStarted += PowerMeter_OnStarted;
                    powerMeter.OnStopped += PowerMeter_OnStopped;
                    powerMeter.OnMeasured += PowerMeter_OnMeasured;
                    powerMeter.OnCleared += PowerMeter_OnCleared;
                }     
            }
        }
        private IPowerMeter powerMeter;
            

        IDInput myDIExt1;
        IDInput myDILaserPort;
        IDOutput myDOExt1;
        IDOutput myDOExt2;
        IDOutput myDOLaserPort;

        /// <summary>
        /// Usercontrol for Treeview
        /// </summary>
        public SpiralLab.Sirius2.Winforms.UI.TreeViewUserControl TreeViewCtrl
        { 
            get { return treeViewControl1; } 
        }
        /// <summary>
        /// Usercontrol for Treeview with block
        /// </summary>
        public SpiralLab.Sirius2.Winforms.UI.TreeViewBlockUserControl TreeViewBlockCtrl
        {
            get { return treeViewBlockControl1; }
        }
        /// <summary>
        /// Usercontrol for propertygrid
        /// </summary>
        public SpiralLab.Sirius2.Winforms.UI.PropertyGridUserControl PropertyGridCtrl
        {
            get { return propertyGridControl1; }
        }
        /// <summary>
        /// Usercontrol for editor
        /// </summary>
        public SpiralLab.Sirius2.Winforms.UI.EditorUserControl EditorCtrl
        {
            get { return editorControl1; }
        }
        /// <summary>
        /// Usercontrol for pen
        /// </summary>
        public SpiralLab.Sirius2.Winforms.UI.PenUserControl PenCtrl
        {
            get { return penControl1; }
        }
        /// <summary>
        /// Usercontrol for laser
        /// </summary>
        public SpiralLab.Sirius2.Winforms.UI.LaserUserControl LaserCtrl
        {
            get { return laserControl1; }
        }
        /// <summary>
        /// Usercontrol for rtc
        /// </summary>
        public SpiralLab.Sirius2.Winforms.UI.RtcUserControl RtcCtrl
        {
            get { return rtcControl1; }
        }
        /// <summary>
        /// Usercontrol for marker
        /// </summary>
        public SpiralLab.Sirius2.Winforms.UI.MarkerUserControl MarkerCtrl
        {
            get { return markerControl1; }
        }
        /// <summary>
        /// Usercontrol for offset
        /// </summary>
        public SpiralLab.Sirius2.Winforms.UI.OffsetUserControl OffsetCtrl
        {
            get { return offsetControl1; }
        }
        /// <summary>
        /// Usercontrol for DI
        /// </summary>
        public SpiralLab.Sirius2.Winforms.UI.RtcDIUserControl RtcDICtrl
        {
            get { return rtcDIUserControl1; }
        }
        /// <summary>
        /// Usercontrol for DO
        /// </summary>
        public SpiralLab.Sirius2.Winforms.UI.RtcDOUserControl RtcDOCtrl
        {
            get { return rtcDOUserControl1; }
        }
        /// <summary>
        /// Usercontrol for manual
        /// </summary>
        public SpiralLab.Sirius2.Winforms.UI.ManualUserControl ManualCtrl
        {
            get { return manualUserControl1; }
        }
        /// <summary>
        /// Usercontrol for remote
        /// </summary>
        public SpiralLab.Sirius2.Winforms.UI.RemoteUserControl RemoteCtrl
        {
            get { return remoteUserControl1; }
        }
        /// <summary>
        /// Usercontrol for powermeter
        /// </summary>
        public SpiralLab.Sirius2.Winforms.UI.PowerMeterControl PowerMeterCtrl
        {
            get { return powerMeterControl1; }
        }
        /// <summary>
        /// Usercontrol for powermap
        /// </summary>
        public SpiralLab.Sirius2.Winforms.UI.PowerMapControl PowerMapCtrl
        {
            get { return powerMapControl1; }
        }
        /// <summary>
        /// Usercontrol for log
        /// </summary>
        public SpiralLab.Sirius2.Winforms.UI.LogUserControl LogCtrl
        {
            get { return logControl1; }
        }

        System.Windows.Forms.Timer timerProgress = new System.Windows.Forms.Timer();
        System.Windows.Forms.Timer timerStatus  = new System.Windows.Forms.Timer();
        Stopwatch timerProgressStopwatch = new Stopwatch();


        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// Create devices likes as <c>IRtc</c>, <c>ILaser</c> and <c>IMarker</c> and assign. <br/>
        /// Digital I/O devices likes as <c>DInput</c>s, <c>DInput</c>s are created when assign <c>IRtc</c> by automatically. <br/>
        /// Create <c>IMarker</c> and assign. <br/>
        /// <c>IDocument</c> is created by automatically. <br/>
        /// </remarks>
        public SiriusEditorUserControl()
        {
            InitializeComponent();

        
            VisibleChanged += SiriusEditorUserControl_VisibleChanged;
            Disposed += SiriusEditorUserControl_Disposed;

            tabControl1.SelectedIndexChanged += TabControl1_SelectedIndexChanged;
            timerProgress.Interval = 100;
            timerProgress.Tick += TimerProgress_Tick;
            timerStatus.Interval = 100;
            timerStatus.Tick += TimerStatus_Tick;
            lblEncoder.DoubleClick += LblEncoder_DoubleClick;
            lblEncoder.DoubleClickEnabled = true;

            lblHelp.Click += LblHelp_Click;
            btnAbout.Click += BtnAbout_Click;
            btnNew.Click += BtnNew_Click;
            btnOpen.Click += BtnOpen_Click;
            btnImportFile.Click += BtnImportFile_Click;
            btnSave.Click += BtnSave_Click;
            btnCopy.Click += BtnCopy_Click;
            btnCut.Click += BtnCut_Click;
            btnPaste.Click += BtnPaste_Click;
            btnPasteArray.Click += BtnPasteArray_Click;
            btnZoomIn.Click += BtnZoomIn_Click;
            btnZoomOut.Click += BtnZoomOut_Click;
            btnZoomFit.Click += BtnZoomFit_Click;
            btnDelete.Click += BtnDelete_Click;

            btnLine.Click += BtnLine_Click;
            btnPoint.Click += BtnPoint_Click;
            btnPoints.Click += BtnPoints_Click;
            btnRectangle.Click += BtnRectangle_Click;
            btnArc.Click += BtnArc_Click;
            btnCircle.Click += BtnCircle_Click;
            btnTrepan.Click += BtnTrepan_Click;
            btnSpiral.Click += BtnSpiral_Click;
            btnText.Click += BtnText_Click;
            btnImageText.Click += BtnImageText_Click;
            btnRaster.Click += BtnRaster_Click;
            btnCircularText.Click += BtnCircularText_Click;
            btnCharacterSetText.Click += BtnCharacterSetText_Click;
            btnSiriusText.Click += BtnSiriusText_Click;
            btnSiriusCharacterSetText.Click += BtnSiriusCharacterSetText_Click;

            mnuDataMatrix.Click += MnuDataMatrix_Click;
            mnuQRCode.Click += MnuQRCode_Click;
            mnuPDF417.Click += MnuPDF417_Click;
            mnuBarcode1D.Click += MnuBarcode1D_Click;
            mnuMeasurementBeginEnd.Click += MnuMeasurementBeginEnd_Click;
            mnuTimer.Click += MnuTimer_Click;
            mnuJumpTo.Click += MnuJumpTo_Click;

            mnuMoFXYBeginEnd.Click += MnuMofXYBeginEnd_Click;
            mnuMoFXYWait.Click += MnuMofXYWait_Click;
            mnuMoFAngularBeginEnd.Click += MnuMofAngularBeginEnd_Click;
            mnuMoFAngularWait.Click += MnuMofAngularWait_Click;
            mnuMoFExternalStartDelay.Click += MnuMoFExternalStartDelay_Click;

            mnuZDelta.Click += MnuZDelta_Click;
            mnuZDefocus.Click += MnuZDefocus_Click;

            mnuMarginLeft.Click += MnuMarginLeft_Click;
            mnuMarginRight.Click += MnuMarginRight_Click;
            mnuMarginTop.Click += MnuMarginTop_Click;
            mnuMarginBottom.Click += MnuMarginBottom_Click;
            mnuAlcDefinedVector.Click += MnuAlcDefinedVector_Click;

            mnuWriteData.Click += MnuWriteData_Click;
            mnuWriteDataExt16.Click += MnuWriteDataExt16_Click;
            mnuWriteDataExt16Cond.Click += MnuWriteDataExt16Cond_Click;
            mnuWaitDataExt16Cond.Click += MnuWaitDataExt16Cond_Click;
        }
        /// <inheritdoc/>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            InternalOnLoad(e);
        }
        /// <inheritdoc/>
        public override void Refresh()
        {
            base.Refresh();
            PropertyGridCtrl.Refresh();
            OffsetCtrl.Refresh();
            EditorCtrl.View.Render();
        }

        /// <inheritdoc/>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.F5))
            {
                if (null != Marker)
                {
                    Marker.Stop();
                    return true;
                }
            }
            else if (keyData == (Keys.F5))
            {
                if (null != Marker)
                {
                    if (!Marker.IsBusy)
                    {
                        var form = new SpiralLab.Sirius2.Winforms.UI.MessageBox($"Do you really want to start mark ?{Environment.NewLine}Target: {Marker.MarkTarget}, Procedure: {Marker.MarkProcedure}{Environment.NewLine}Offset(s): {Marker.Offsets.Length}", "Warning", MessageBoxButtons.YesNo);
                        DialogResult dialogResult = form.ShowDialog(this);
                        if (dialogResult == DialogResult.Yes)
                        {
                            Marker.Start();
                            return true;
                        }
                    }
                }
            }
            else if (keyData == (Keys.F6))
            {
                if (null != Marker)
                {
                    Marker.Reset();
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void InternalOnLoad(EventArgs e)
        {
            TreeViewCtrl.View = EditorCtrl.View;
            TreeViewBlockCtrl.View = EditorCtrl.View;
            PropertyGridCtrl.View = EditorCtrl.View;

            // Create one by default
            this.Document = new DocumentBase();
            // New document by default
            Document.ActNew();

            PowerMapCtrl.Document = this.Document;
        }
        private void SiriusEditorUserControl_VisibleChanged(object sender, EventArgs e)
        {
            timerStatus.Enabled = Visible;
        }
        private void BtnSiriusCharacterSetText_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateSiriusCharacterSetText(Config.DefaultSiriusFont, CharacterSetFormats.Date, 5);
            document.ActAdd(entity);
        }
        private void BtnCharacterSetText_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateCharacterSetText(Config.DefaultFont, CharacterSetFormats.Date, 5);
            document.ActAdd(entity);
        }     
        private void BtnCircularText_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateCircularText(Config.DefaultFont, "POWERED BY SIRIUS2 0123456789", FontStyle.Regular, 2,  TextCircularDirections.ClockWise, 5, 180);
            document.ActAdd(entity);
        }
        private void BtnSiriusText_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateSiriusText(Config.DefaultSiriusFont, "SIRIUS2", 2.5);
            document.ActAdd(entity);
        }

        private void MnuQRCode_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateQRCode("SIRIUS2", Barcode2DCells.Dots, 3, 2, 2);
            document.ActAdd(entity);
        }
        private void MnuDataMatrix_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateDataMatrix("SIRIUS2", Barcode2DCells.Dots, 3, 2, 2);
            document.ActAdd(entity);
        }
        private void MnuPDF417_Click(object sender, EventArgs e)
        {
            double height = 2;
            var entity = EntityFactory.CreatePDF417("SIRIUS2", Barcode2DCells.Dots, 3, height * 3.75, height);
            document.ActAdd(entity);
        }
        private void MnuBarcode1D_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateBarcode("1234567890", Barcode1DFormats.Code128, 3, 5, 2);
            document.ActAdd(entity);
        }
        private void MnuWriteDataExt16_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateWriteDataExt16(0, false);
            document.ActAdd(entity);
        }
        private void MnuWriteDataExt16Cond_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateWriteDataExt16Cond("0000 0000 0000 0000", "0000 0000 0000 0000", "0000 0000 0000 0000", false);
            document.ActAdd(entity);
        }
        private void MnuWaitDataExt16Cond_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateWaitDataExt16Cond("0000 0000 0000 0000", "0000 0000 0000 0000");
            document.ActAdd(entity);
        }
        private void MnuWriteData_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateWriteData(ExtensionChannels.ExtAO2, 0);
            document.ActAdd(entity);
        }
        private void MnuAlcDefinedVector_Click(object sender, EventArgs e)
        {
            {
                var entity = EntityFactory.CreateRampEnd();
                document.ActAdd(entity);
            }
            {
                var entity = EntityFactory.CreateRampBegin(AutoLaserControlSignals.Frequency, 50 * 1000);
                document.ActInsert(entity, document.ActiveLayer, 0);
            }
        }

        private void SiriusEditorUserControl_Disposed(object sender, EventArgs e)
        {
            timerStatus.Enabled = false;
            timerProgress.Enabled = false;
            timerStatus.Tick -= TimerStatus_Tick;
            timerProgress.Tick -= TimerProgress_Tick;
        }

        private void MenuVisibility()
        {
            Debug.Assert(rtc != null);
            if (null == rtc)
                return;

            switch (rtc.RtcType)
            {
                case RtcTypes.RtcVirtual:
                    break;
                case RtcTypes.Rtc4:
                case RtcTypes.Rtc5:
                case RtcTypes.Rtc6:
                case RtcTypes.Rtc6e:
                    break;
                case RtcTypes.Rtc6SyncAxis:
                    btnImageText.Enabled = false;
                    mnuMeasurementBeginEnd.Enabled = false;
                    mnuMoF.Enabled = false;
                    mnuZDelta.Enabled = false;                         
                    mnuZDefocus.Enabled = false;
                    mnuWriteDataExt16Cond.Enabled = false;
                    mnuWaitDataExt16Cond.Enabled = false;

                    lblEncoder.Visible = false;
                    btnCharacterSetText.Enabled = false;
                    btnSiriusCharacterSetText.Enabled = false;
                    break;
            }
        }
        private void EntityVisibility()
        {
            EntityPen.PropertyVisibility(rtc);
            EntityPen.PropertyVisibility(laser);
            EntityLayer.PropertyVisibility(rtc);
            EntityPoints.PropertyVisibility(rtc);
            EntityRampBegin.PropertyVisibility(rtc);
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (null == Document)
                return;
            Document.ActSelectClear();
            switch(tabControl1.SelectedIndex)
            {
                case 0:
                    EditorCtrl.View.ViewMode = ViewModes.Entity;
                    break;
                case 1:
                    EditorCtrl.View.ViewMode = ViewModes.Block;
                    break;
            }
            EditorCtrl.View.Render();
        }

        private void MnuMarginBottom_Click(object sender, EventArgs e)
        {
            document.ActAlign(document.Selected, MarginAlignments.Bottom);
            DoRender();
        }

        private void MnuMarginTop_Click(object sender, EventArgs e)
        {
            document.ActAlign(document.Selected, MarginAlignments.Top);
            DoRender();
        }

        private void MnuMarginRight_Click(object sender, EventArgs e)
        {
            document.ActAlign(document.Selected, MarginAlignments.Right);
            DoRender();
        }

        private void MnuMarginLeft_Click(object sender, EventArgs e)
        {
            document.ActAlign(document.Selected, MarginAlignments.Left);
            DoRender();
        }

        private void MnuZDelta_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateZDelta(0);
            document.ActAdd(entity);
        }

        private void MnuZDefocus_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateZDefocus(0);
            document.ActAdd(entity);
        }

        private void LblEncoder_DoubleClick(object sender, EventArgs e)
        {
            if (null == Rtc)
                return;
            var rtcMoF = Rtc as IRtcMoF;
            if (rtcMoF == null)
                return;

            var form = new SpiralLab.Sirius2.Winforms.UI.MessageBox($"Do you really want to reset encoder values ?", "Warning", MessageBoxButtons.YesNo);
            DialogResult dialogResult = form.ShowDialog(this);
            if (dialogResult != DialogResult.Yes)
                return;

            rtcMoF.CtlMofEncoderReset();
        }

        private void MnuMofAngularWait_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateMoFWait(RtcEncoderWaitConditions.Over, 90);
            document.ActAdd(entity);
        }

        private void MnuMofAngularBeginEnd_Click(object sender, EventArgs e)
        {
            {
                var entity = EntityFactory.CreateMoFEnd(Vector2.Zero);
                document.ActAdd(entity);
                
            }
            {
                var entity = EntityFactory.CreateMoFBegin(RtcEncoderTypes.Angular);
                document.ActInsert(entity, document.ActiveLayer, 0);
            }
        }

        private void MnuMofXYWait_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateMoFWait(RtcEncoders.EncX, RtcEncoderWaitConditions.Over, 10);
            document.ActAdd(entity);
        }

        private void MnuMofXYBeginEnd_Click(object sender, EventArgs e)
        {
            {
                var entity = EntityFactory.CreateMoFEnd(Vector2.Zero);
                document.ActAdd(entity);
            }
            {
                var entity = EntityFactory.CreateMoFBegin(RtcEncoderTypes.XY);
                document.ActInsert(entity, document.ActiveLayer, 0);
            }
        }
        private void MnuMoFExternalStartDelay_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateMoFExternalStartDelay( RtcEncoders.EncX, 0);
            document.ActAdd(entity);
        }

        private void BtnPoints_Click(object sender, EventArgs e)
        {
            Vector2[] locations = new Vector2[]
            {
                new Vector2(-1,1),
                new Vector2(-1.1f,1.2f),
                new Vector2(-1.2f,1.0f),
                new Vector2(-0.9f,-0.8f),
                new Vector2(-1,1.1f),
                new Vector2(-1.2f,0.7f),
                new Vector2(-1.4f,1.1f),
                new Vector2(-1.2f,-0.95f),
            };
            var entity = EntityFactory.CreatePoints(locations, 10);
            document.ActAdd(entity);
        }

        private void BtnPoint_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreatePoint(0, 0, 10);
            document.ActAdd(entity);
        }

        private void BtnLine_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateLine(-10, 0, 10, 0);
            document.ActAdd(entity);
        }

        private void BtnImportFile_Click(object sender, EventArgs e)
        {
            //var dlg = new OpenFileDialog();
            //dlg.Filter = Config.FileImportModelFilters;
            //dlg.Title = "Import Model File";
            //dlg.InitialDirectory = SpiralLab.Sirius2.Winforms.Config.SamplePath;
            //DialogResult result = dlg.ShowDialog();
            //if (result != DialogResult.OK)
            //    return;
            //Cursor.Current = Cursors.WaitCursor;
            //Document.ActImport(dlg.FileName, out var entity);
            //Cursor.Current = Cursors.Default;
            
            // or preview import winform
            var form = new SpiralLab.Sirius2.Winforms.UI.ImportForm();
            DialogResult dialogResult = form.ShowDialog(this);
            if (dialogResult != DialogResult.OK)
                return;
            if (null == form.Entity)
                return;

            // to make gl render as current
            //if (View is ViewBase vb)
            //    vb.Renderer.MakeCurrent();
            //or
            DoRender();

            Cursor.Current = Cursors.WaitCursor;
            var cloned = (IEntity)form.Entity.Clone();
            cloned.IsNeedToRegen = true;
            cloned.Parent = null;
            cloned.IsSelected = false;
            Document.ActAdd(cloned);
            Cursor.Current = Cursors.Default;
        }

        private void BtnRectangle_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateRectangle(Vector2.Zero, 10,10);
            document.ActAdd(entity);
        }

        private void MnuTimer_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateTimer(1);
            document.ActAdd(entity);
        }
        private void MnuJumpTo_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateJumpTo(Vector3.Zero);
            document.ActAdd(entity);
        }

        private void BtnText_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateText(Config.DefaultFont, $"Hello{Environment.NewLine}SIRIUS2", FontStyle.Regular, 2);
            Document.ActAdd(entity);
        }

        private void BtnImageText_Click(object sender, EventArgs e)
        {
            var form = new SpiralLab.Sirius2.Winforms.UI.ImageTextForm();
            if (DialogResult.OK != form.ShowDialog())
                return;
            var entity = EntityFactory.CreateImageText(form.FontName, form.ImageText, form.Style, form.IsFill, form.OutlinePixel, form.HeightPixel, 5, 10);
            Document.ActAdd(entity);
        }
        private void BtnRaster_Click(object sender, EventArgs e)
        {
            //var entity = new EntityRaster(2, 2, 100, 100);
            //Document.ActAdd(entity);
            var dlg = new OpenFileDialog();
            dlg.Filter = Config.FileImportImageFilters;
            dlg.InitialDirectory = Config.SamplePath;
            dlg.Title = "Open Image File";

            DialogResult result = dlg.ShowDialog();
            if (result != DialogResult.OK)
                return;
          
            var entity = new EntityRaster(2, dlg.FileName);
            Document.ActAdd(entity);
        }
        private void BtnDivide_Click(object sender, EventArgs e)
        {
            //if (document.Selected.Length > 0)
            //    Document.ActDivide(document.Selected, null);
        }

        private void BtnZoomIn_Click(object sender, EventArgs e)
        {

            EditorCtrl.View.Camera.ZoomIn(Point.Empty);
            DoRender();
        }
        private void BtnZoomOut_Click(object sender, EventArgs e)
        {
            EditorCtrl.View.Camera.ZoomOut(Point.Empty);
            DoRender();
        }
        private void BtnZoomFit_Click(object sender, EventArgs e)
        {
            if (0 == Document.Selected.Length)
            {
                var bbox = BoundingBox.RealBoundingBox(Document.InternalData.Layers.ToArray());
                EditorCtrl.View.Camera.ZoomFit(bbox);
            }
            else
            {
                var bbox = BoundingBox.RealBoundingBox(Document.Selected);
                EditorCtrl.View.Camera.ZoomFit(bbox);
            }
            DoRender();
        }

        private void BtnPasteArray_Click(object sender, EventArgs e)
        {
            if (Document.Clipboard == null || Document.Clipboard.Length == 0)
            {
                var form = new SpiralLab.Sirius2.Winforms.UI.MessageBox($"Clipboard are empty. Please copy or cut at first", "Warning", MessageBoxButtons.OK);
                form.ShowDialog(this);
                return;
            }
            {
                var form = new SpiralLab.Sirius2.Winforms.UI.ArrayForm();
                if (DialogResult.OK != form.ShowDialog(this))
                    return;
                foreach (var o in form.Calcuated)
                {
                    IEntity[] pastedEntities = Document.ActPaste(null);
                    foreach (var entity in pastedEntities)
                    {
                        entity.Translate(o.Dx, o.Dy);
                    }
                }
            }
            DoRender();
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            Document.ActCopy();
        }
        private void BtnPaste_Click(object sender, EventArgs e)
        {
            Document.ActPaste(null);
        }
        private void BtnCut_Click(object sender, EventArgs e)
        {
            Document.ActCut();
        }
        private void BtnAbout_Click(object sender, EventArgs e)
        {
            var form = new SpiralLab.Sirius2.Winforms.UI.AboutForm();
            form.ShowDialog();
        }

        private void Document_OnOpened(IDocument document, string fileName)
        {
            if (!this.IsHandleCreated || this.IsDisposed)
                return;
            this.Invoke(new MethodInvoker(delegate ()
            {
                lblFileName.Text = fileName;
                if (null != Laser)
                    foreach (var pen in document.InternalData.Pens)
                        pen.PowerMax = Laser.MaxPowerWatt;
                PropertyGridCtrl.Refresh();
            }));
        }
        private void Document_OnSaved(IDocument document, string fileName)
        {
            if (!this.IsHandleCreated || this.IsDisposed)
                return;
            this.Invoke(new MethodInvoker(delegate ()
            {
                lblFileName.Text = fileName;
            }));
        }
        private void Document_OnSelected(IDocument document, IEntity[] entities)
        {
            if (!this.IsHandleCreated || this.IsDisposed)
                return;
            this.Invoke(new MethodInvoker(delegate ()
            {
                lblSelected.Text = $"Selected: {entities.Length}";
            }));
        }

        private void Renderer_Paint(object sender, PaintEventArgs e)
        {
            DoRender();
        }
        private void Renderer_MouseMove(object sender, MouseEventArgs e)
        {
            var intersect = OpenTKHelper.ScreenToWorldPlaneZIntersect(e.Location, Vector3.Zero, EditorCtrl.View.Camera.ViewMatrix, EditorCtrl.View.Camera.ProjectionMatrix);
            lblPos.Text = $"XY: {intersect.X:F3}, {intersect.Y:F3}  P: {e.Location.X}, {e.Location.Y}";

        }
        private void Mof_OnEncoderChanged(IRtcMoF rtcMoF, int encX, int encY)
        {
            if (!this.IsHandleCreated || this.IsDisposed)
                return;
            this.Invoke(new MethodInvoker(delegate ()
            {
                switch (rtcMoF.EncoderType)
                {
                    default:
                    case RtcEncoderTypes.XY:
                        {
                            rtcMoF.CtlMofGetEncoder(out var x, out var y, out var xMm, out var yMm);
                            lblEncoder.Text = $"ENC XY: {x}, {y} [{xMm:F3}, {yMm:F3}]";
                        }
                        break;
                    case RtcEncoderTypes.Angular:
                        { 
                            rtcMoF.CtlMofGetAngularEncoder(out var x, out var angle);
                            lblEncoder.Text = $"ENC X,0: {x} [{angle:F3}˚]";
                        }
                        break;
                }
            }));
        }

        private void BtnNew_Click(object sender, EventArgs e)
        {
            if (document.IsModified)
            {
                var form = new SpiralLab.Sirius2.Winforms.UI.MessageBox($"Do you really want to new document without save ?", "Warning", MessageBoxButtons.YesNo);
                DialogResult dialogResult = form.ShowDialog(this);
                if (dialogResult != DialogResult.Yes)
                    return;
            }
            document.ActNew();
        }
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            document.ActRemove(document.Selected);
        }
        private void BtnOpen_Click(object sender, EventArgs e)
        {
            if (Config.NotifyOpen(this))
                return;
            var dlg = new OpenFileDialog();
            dlg.Filter = Config.FileOpenFilters;
            dlg.Title = "Open File";
            DialogResult result = dlg.ShowDialog();
            if (result != DialogResult.OK)
                return;
            if (Document.IsModified)
            {
                var form = new SpiralLab.Sirius2.Winforms.UI.MessageBox($"Do you really want to open without save ?", "Warning", MessageBoxButtons.YesNo);
                DialogResult dialogResult = form.ShowDialog(this);
                if (dialogResult != DialogResult.Yes)
                    return;
            }
            Cursor.Current = Cursors.WaitCursor;
            document.ActOpen(dlg.FileName);
            Cursor.Current = Cursors.Default;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (Config.NotifySave(this))
                return;
            var dlg = new SaveFileDialog();
            dlg.Filter = Config.FileSaveFilters;
            dlg.Title = "Save File";
            dlg.OverwritePrompt = true;
            DialogResult result = dlg.ShowDialog();
            if (result != DialogResult.OK)
                return;
            Cursor.Current = Cursors.WaitCursor;
            document.ActSave(dlg.FileName);
            Cursor.Current = Cursors.Default;
        }

        private void BtnArc_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateArc(Vector2.Zero, 10, 0, 180);
            document.ActAdd(entity);
        }
        private void BtnCircle_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateArc(Vector2.Zero, 10, 0, 360);
            document.ActAdd(entity);
        }
        private void BtnSpiral_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateSpiral(Vector2.Zero, 1, 5, 0, 10, true);
            document.ActAdd(entity);
        }
        private void BtnTrepan_Click(object sender, EventArgs e)
        {
            var entity = EntityFactory.CreateTrepan(Vector2.Zero, 1, 5, 10);
            document.ActAdd(entity);
        }

        private void MnuMeasurementBeginEnd_Click(object sender, EventArgs e)
        {
            if (rtc is Rtc5)
            {
                var entity1 = EntityFactory.CreateMeasurementEnd();
                document.ActAdd(entity1);
                var channels = new MeasurementChannels[4]
                {
                    MeasurementChannels.SampleX,
                    MeasurementChannels.SampleY,
                    MeasurementChannels.SampleZ,
                    MeasurementChannels.LaserOn,
                };
                var entity2 = EntityFactory.CreateMeasurementBegin(5 * 1000, channels);
                document.ActInsert(entity2, document.ActiveLayer, 0);
            }
            else if (rtc is Rtc6)
            {
                var entity1 = EntityFactory.CreateMeasurementEnd();
                document.ActAdd(entity1);
                var channels = new MeasurementChannels[8]
                {
                    MeasurementChannels.SampleX,
                    MeasurementChannels.SampleY,
                    MeasurementChannels.SampleZ,
                    MeasurementChannels.LaserOn,
                    MeasurementChannels.OutputPeriod,
                    MeasurementChannels.PulseLength,
                    MeasurementChannels.Enc0Counter,
                    MeasurementChannels.Enc1Counter,
                };
                var entity2 = EntityFactory.CreateMeasurementBegin(5 * 1000, channels);
                document.ActInsert(entity2, document.ActiveLayer, 0);
            }
        }
        private void LblHelp_Click(object sender, EventArgs e)
        {
            var form = new SpiralLab.Sirius2.Winforms.UI.MessageBox(Config.KeyboardHelpMessage, "Help - Keyboards", MessageBoxButtons.OK);
            DialogResult dialogResult = form.ShowDialog(this);
        }

        private void EnableDisableControlByMarking(bool enable)
        {
            if (!this.IsHandleCreated || this.IsDisposed)
                return;
            if (!IsDisableControl)
            {
                tlsTop.Enabled = enable;
                tlsTop2.Enabled = enable;
                TreeViewCtrl.Enabled = enable;
                TreeViewBlockCtrl.Enabled = enable;
                EditorCtrl.Enabled = enable;
                PenCtrl.Enabled = enable;
                LaserCtrl.Enabled = enable;
                RtcCtrl.Enabled = enable;
                OffsetCtrl.Enabled = enable;
                PropertyGridCtrl.Enabled = enable;
            }
            DoRender();
        }

        int timerStatusColorCounts = 0;
        private void TimerStatus_Tick(object sender, EventArgs e)
        {
            if (null == this.Marker)
                return;
            if (this.Marker.IsReady)
            {
                lblReady.ForeColor = Color.Black;
                lblReady.BackColor = Color.Lime;
            }
            else
            {
                lblReady.ForeColor = Color.White;
                lblReady.BackColor = Color.Green;
            }

            if (this.Marker.IsBusy)
            {
                timerStatusColorCounts = checked(timerStatusColorCounts + 1);
                if (0 == timerStatusColorCounts % 2)
                {
                    lblBusy.BackColor = Color.Red;
                    lblBusy.ForeColor = Color.White;
                }
                else
                {
                    lblBusy.BackColor = Color.Maroon;
                    lblBusy.ForeColor = Color.White;
                }
            }
            else
            {
                lblBusy.ForeColor = Color.White;
                lblBusy.BackColor = Color.Maroon;
                timerStatusColorCounts = 0;
            }

            if (this.Marker.IsError)
            {
                lblError.ForeColor = Color.White;
                lblError.BackColor = Color.Red;
            }
            else
            {
                lblError.ForeColor = Color.White;
                lblError.BackColor = Color.Maroon;
            }
            if (null == this.Remote || !Remote.IsConnected)
            {
                lblConnect.Text = " CONNECT ";
                lblConnect.ForeColor = Color.White;
                lblConnect.BackColor = Color.Maroon;
            }
            else
            {
                lblConnect.ForeColor = Color.Black;
                switch(Remote.ControlMode)
                {
                    case ControlModes.Local:
                        lblConnect.Text = " CONNECT /LOCAL ";
                        lblConnect.BackColor = Color.Yellow;
                        break;
                    case ControlModes.Remote:
                        lblConnect.Text = " CONNECT /REMOTE ";
                        lblConnect.BackColor = Color.Lime;
                        break;
                }
            }

            if (null != EditorCtrl.View)
                lblRenderTime.Text = $"Render: {EditorCtrl.View.RenderTime} ms";
        }

        int timerProgressColorCounts = 0;
        private void TimerProgress_Tick(object sender, EventArgs e)
        {
            if (!this.IsHandleCreated || this.IsDisposed)
                return;
            this.Invoke(new MethodInvoker(delegate ()
            {
                if (0 == timerProgressColorCounts++ % 2)
                    lblProcessTime.ForeColor = statusStrip1.ForeColor;
                else
                    lblProcessTime.ForeColor = Color.Red;

                lblProcessTime.Text = $"{timerProgressStopwatch.ElapsedMilliseconds / 1000.0:F1} sec";
            }));
        }
        private void Marker_OnStarted(IMarker marker)
        {
            if (!this.IsHandleCreated || this.IsDisposed)
                return;
            this.Invoke(new MethodInvoker(delegate ()
            {
                timerProgressStopwatch.Restart();
                timerProgress.Start();
                EnableDisableControlByMarking(false);
            }));
        }
        private void Marker_OnEnded(IMarker marker, bool success, TimeSpan ts)
        {
            if (!this.IsHandleCreated || this.IsDisposed)
                return;
            this.Invoke(new MethodInvoker(delegate ()
            {
                timerProgressStopwatch.Stop();
                timerProgress.Stop();
                if (success)
                {
                    lblProcessTime.ForeColor = statusStrip1.ForeColor;
                    lblProcessTime.Text = $"{ts.TotalSeconds:F1} sec";
                }
                else
                {
                    lblProcessTime.ForeColor = Color.Red;
                    lblProcessTime.Text = $"{ts.TotalSeconds:F1} sec";
                }
                EnableDisableControlByMarking(true);
                EditorCtrl.Focus();
            }));
        }
        private void PowerMeter_OnCleared(IPowerMeter obj)
        {
            if (!this.IsHandleCreated || this.IsDisposed)
                return;
            this.Invoke(new MethodInvoker(delegate ()
            {
                lblPowerWatt.Text = $"(Empty)";
            }));
        }
        private void PowerMeter_OnStarted(IPowerMeter obj)
        {
            if (!this.IsHandleCreated || this.IsDisposed)
                return;
            this.Invoke(new MethodInvoker(delegate ()
            {
                lblPowerWatt.Text = $"0.0 W";
            }));
        }
        private void PowerMeter_OnStopped(IPowerMeter obj)
        {
            if (!this.IsHandleCreated || this.IsDisposed)
                return;
            this.Invoke(new MethodInvoker(delegate ()
            {
                //lblPowerWatt.Text = $"0.0 W";
            }));
        }
        private void PowerMeter_OnMeasured(IPowerMeter arg1, DateTime dt, double watt)
        {
            if (!this.IsHandleCreated || this.IsDisposed)
                return;
            this.Invoke(new MethodInvoker(delegate ()
            {
                lblPowerWatt.Text = $"{watt:F3} W";
            }));
        }

        /// <summary>
        /// Do <c>IView</c> render
        /// </summary>
        public void DoRender()
        {
            if (!this.IsHandleCreated || this.IsDisposed)
                return;
            this.Invoke(new MethodInvoker(delegate ()
            {
                EditorCtrl.View.Render();
                lblRenderTime.Text = $"Render: {EditorCtrl.View.RenderTime} ms";
            }));
        }
    }
}
