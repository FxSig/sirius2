﻿/*
 *                                                             ,--,      ,--,                              
 *              ,-.----.                                     ,---.'|   ,---.'|                              
 *    .--.--.   \    /  \     ,---,,-.----.      ,---,       |   | :   |   | :      ,---,           ,---,.  
 *   /  /    '. |   :    \ ,`--.' |\    /  \    '  .' \      :   : |   :   : |     '  .' \        ,'  .'  \ 
 *  |  :  /`. / |   |  .\ :|   :  :;   :    \  /  ;    '.    |   ' :   |   ' :    /  ;    '.    ,---.' .' | 
 *  ;  |  |--`  .   :  |: |:   |  '|   | .\ : :  :       \   ;   ; '   ;   ; '   :  :       \   |   |  |: | 
 *  |  :  ;_    |   |   \ :|   :  |.   : |: | :  |   /\   \  '   | |__ '   | |__ :  |   /\   \  :   :  :  / 
 *   \  \    `. |   : .   /'   '  ;|   |  \ : |  :  ' ;.   : |   | :.'||   | :.'||  :  ' ;.   : :   |    ;  
 *    `----.   \;   | |`-' |   |  ||   : .  / |  |  ;/  \   \'   :    ;'   :    ;|  |  ;/  \   \|   :     \ 
 *    __ \  \  ||   | ;    '   :  ;;   | |  \ '  :  | \  \ ,'|   |  ./ |   |  ./ '  :  | \  \ ,'|   |   . | 
 *   /  /`--'  /:   ' |    |   |  '|   | ;\  \|  |  '  '--'  ;   : ;   ;   : ;   |  |  '  '--'  '   :  '; | 
 *  '--'.     / :   : :    '   :  |:   ' | \.'|  :  :        |   ,/    |   ,/    |  :  :        |   |  | ;  
 *    `--'---'  |   | :    ;   |.' :   : :-'  |  | ,'        '---'     '---'     |  | ,'        |   :   /   
 *              `---'.|    '---'   |   |.'    `--''                              `--''          |   | ,'    
 *                `---`            `---'                                                        `----'   
 * 
 * 2023 Copyright to (c)SpiralLAB. All rights reserved.
 * Description : Laser Source (Duty cycle/pulse-width power control)
 * Author : hong chan, choi / hcchoi@spirallab.co.kr (http://spirallab.co.kr)
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using SpiralLab.Sirius2;
using SpiralLab.Sirius2.Laser;
using SpiralLab.Sirius2.PowerMap;
using SpiralLab.Sirius2.Scanner;
using SpiralLab.Sirius2.Scanner.Rtc;

namespace Demos
{
    /// <summary>
    /// Duty cycle (by pulse width) output power control
    /// </summary>
    public class MyDutyCycle
        : ILaser
        , ILaserPowerControl
    {
        /// <inheritdoc/>
        public virtual event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Nofity property value has changed
        /// </summary>
        /// <param name="propertyName">Property name</param>
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            var receivers = this.PropertyChanged?.GetInvocationList();
            if (null != receivers)
                foreach (PropertyChangedEventHandler receiver in receivers)
                    receiver.BeginInvoke(this, new PropertyChangedEventArgs(propertyName), null, null);
        }

        /// <inheritdoc/>  
        [Browsable(false)]
        public virtual object SyncRoot { get; protected set; }

        /// <inheritdoc/>  
        [RefreshProperties(RefreshProperties.All)]
        [Browsable(true)]
        [ReadOnly(true)]
        [Category("Basic")]
        [DisplayName("Index")]
        [Description("Identifier")]
        public virtual int Index { get; set; }

        /// <inheritdoc/>  
        [RefreshProperties(RefreshProperties.All)]
        [Browsable(true)]
        [ReadOnly(true)]
        [Category("Basic")]
        [DisplayName("Name")]
        [Description("Name")]
        public virtual string Name { get; set; }

        /// <inheritdoc/>  
        [Browsable(false)]
        public virtual LaserTypes LaserType { get { return LaserTypes.UserDefined3; } }

        /// <inheritdoc/>  
        [RefreshProperties(RefreshProperties.All)]
        [Browsable(true)]
        [ReadOnly(true)]
        [Category("Power Control")]
        [DisplayName("Map")]
        [Description("Assigned PowerMap")]
        public virtual IPowerMap PowerMap { get; set; }

        /// <inheritdoc/>  
        [RefreshProperties(RefreshProperties.All)]
        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Power Control")]
        [DisplayName("Compensated")]
        [Description("Enable(or Disable) Compensated Output Power by PowerMap")]
        public virtual bool IsCompensated { get; set; } = false;

        /// <inheritdoc/>  
        [RefreshProperties(RefreshProperties.All)]
        [Browsable(true)]
        [ReadOnly(true)]
        [Category("Power Control")]
        [DisplayName("Power (max)")]
        [Description("Max Power (W)")]
        public virtual double MaxPowerWatt { get; set; }

        /// <inheritdoc/>  
        [RefreshProperties(RefreshProperties.All)]
        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Status")]
        [DisplayName("Ready")]
        [Description("Ready Status")]
        public virtual bool IsReady
        {
            get { return isReady; }
            protected set
            {
                if (isReady != value)
                {
                    isReady = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        protected bool isReady;

        /// <inheritdoc/>  
        [RefreshProperties(RefreshProperties.All)]
        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Status")]
        [DisplayName("Busy")]
        [Description("Busy Status")]
        public virtual bool IsBusy
        {
            get { return isBusy; }
            protected set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        protected bool isBusy;

        /// <inheritdoc/>  
        [RefreshProperties(RefreshProperties.All)]
        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Status")]
        [DisplayName("Error")]
        [Description("Error Status")]
        public virtual bool IsError
        {
            get { return isError; }
            protected set
            {
                if (isError != value)
                {
                    isError = value;
                    this.NotifyPropertyChanged();
                    if (isError)
                    {
                        Logger.Log(Logger.Types.Info, $"laser [{this.Index}]: error occurs");
                    }
                }
            }
        }
        protected bool isError;


        /// <inheritdoc/>  
        [Browsable(false)]
        public virtual IScanner Scanner { get; set; }

        /// <inheritdoc/>  
        [Browsable(false)]
        public virtual bool IsPowerControl { get; protected set; }

        /// <inheritdoc/>  
        [Browsable(false)]
        public virtual bool IsGuideControl { get; protected set; }

        [RefreshProperties(RefreshProperties.All)]
        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Power Control")]
        [DisplayName("Method")]
        [Description("Laser Power Control Method")]
        public virtual PowerControlMethods PowerControlMethod { get; set; }

        /// <inheritdoc/>  
        [RefreshProperties(RefreshProperties.All)]
        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Power Control")]
        [DisplayName("Delay")]
        [Description("Power Control Delay Time (msec)")]
        public virtual double PowerControlDelayTime { get; set; }

        /// <inheritdoc/>  
        [RefreshProperties(RefreshProperties.All)]
        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Power Control")]
        [DisplayName("Power (last)")]
        [Description("Last Commanded Power (W)")]
        public virtual double LastPowerWatt
        {
            get { return laserPowerWatt; }
            protected set
            {
                if (laserPowerWatt != value)
                {
                    laserPowerWatt = value;
                    this.NotifyPropertyChanged();
                }
            }
        }
        protected double laserPowerWatt;

        /// <summary>
        /// Min Duty Cycle Duty (%)
        /// <para>Default: 1</para>
        /// </summary>
        [RefreshProperties(RefreshProperties.All)]
        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Control (duty cycle)")]
        [DisplayName("Min duty cycle")]
        [Description("Min Duty Cycle (%)")]
        public virtual double MinDutyCycle { get; set; } = 1;

        /// <summary>
        /// Max Duty Cycle Duty (%)
        /// <para>Default: 99</para>
        /// </summary>
        [RefreshProperties(RefreshProperties.All)]
        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Control (duty cycle)")]
        [DisplayName("Max duty cycle")]
        [Description("Max Duty Cycle (%)")]
        public virtual double MaxDutyCycle { get; set; } = 100;


        /// <inheritdoc/>  
        [Browsable(false)]
        public virtual object Tag { get; set; }

        private bool disposed = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public MyDutyCycle()
        {
            this.SyncRoot = new object();
            this.Name = "My Laser";
            this.IsPowerControl = true;
            // RTC ANALOG1 Port Output 
            this.PowerControlMethod = PowerControlMethods.DutyCycle;
            this.PowerControlDelayTime = 1;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="index">Identifier</param>
        /// <param name="name">Name</param>
        /// <param name="maxPowerWatt">Max power (W)</param>
        public MyDutyCycle(int index, string name, double maxPowerWatt)
            : this()
        {
            this.Index = index;
            this.Name = name;
            this.MaxPowerWatt = maxPowerWatt;
        }
        /// <summary>
        /// Finalizer
        /// </summary>
        ~MyDutyCycle()
        {
            this.Dispose(false);
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Dispose internal resources
        /// </summary>
        /// <param name="disposing">Explicit dispose or not</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed)
                return;
            if (disposing)
            {
            }
            this.disposed = true;
        }
        /// <inheritdoc/> 
        public virtual bool CheckErrors()
        {
            //IsError = 
            return true;
        }
        /// <inheritdoc/> 
        public virtual bool CheckReady()
        {
            //IsReady = 
            return true;
        }
        /// <inheritdoc/> 
        public virtual bool CheckBusy()
        {
            //IsBusy = 
            return true;
        }

        /// <inheritdoc/>  
        public bool Initialize()
        {
            LastPowerWatt = 0;
            return true;
        }
        /// <inheritdoc/>  
        public virtual bool CtlAbort()
        {
            lock (SyncRoot)
            {
                //prevPowerWatt = 0;
            }
            return true;
        }
        /// <inheritdoc/>  
        public virtual bool CtlReset()
        {
            lock (SyncRoot)
            {
                //IsTimedOut = false;
                IsError = false;
                return true;
            }
        }

        #region ILaserPowerControl impl
        /// <inheritdoc/>  
        public virtual bool CtlPower(double targetWatt, string category = "")
        {
            if (!this.IsPowerControl)
                return true;
            Debug.Assert(this.MaxPowerWatt > 0);
            var rtc = Scanner as IRtc;
            Debug.Assert(rtc != null);
            bool success = true;
            double compensatedWatt = targetWatt;
            if (null != PowerMap && IsCompensated && !string.IsNullOrEmpty(category))
            {
                success &= PowerMap.Compensate(category, targetWatt, out compensatedWatt);
                if (!success)
                    return false;
            }
            lock (SyncRoot)
            {
                double percentage = compensatedWatt / this.MaxPowerWatt * 100.0;
                if (percentage > 100)
                    percentage = 100;
                if (percentage < 0)
                    percentage = 0;
                switch (this.PowerControlMethod)
                {
                    default:
                        Logger.Log(Logger.Types.Error, $"laser [{this.Index}]: unsupported !");
                        return false;
                    case PowerControlMethods.DutyCycle:
                        double dutyCycle = this.MinDutyCycle + (this.MaxDutyCycle - this.MinDutyCycle) * percentage / 100.0;
                        double period = 1.0 / rtc.Frequency * (double)1.0e6; //usec
                        double tempPulseWidth = period * dutyCycle / 100.0;
                        success &= rtc.CtlFrequency(rtc.Frequency, (double)tempPulseWidth);
                        break;
                }
            }
            Thread.Sleep((int)this.PowerControlDelayTime);
            if (success)
            {
                LastPowerWatt = targetWatt;
                Logger.Log(Logger.Types.Warn, $"laser [{this.Index}]: power(W): {targetWatt:F3} [{compensatedWatt:F3}]");
            }
            else
            {
                Logger.Log(Logger.Types.Error, $"laser [{this.Index}]: fail to set power(W): {targetWatt:F3}");
            }
            return success;
        }

        /// <inheritdoc/>  
        public virtual bool ListBegin()
        {
            return true;
        }
        /// <inheritdoc/>  
        public virtual bool ListEnd()
        {
            return true;
        }
        /// <inheritdoc/>  
        public virtual bool ListPower(double targetWatt, string category = "")
        {
            if (!this.IsPowerControl)
                return true;
            Debug.Assert(this.MaxPowerWatt > 0);
            var rtc = Scanner as IRtc;
            Debug.Assert(rtc != null);
            double compensatedWatt = targetWatt;
            bool success = true;
            if (null != PowerMap && IsCompensated && !string.IsNullOrEmpty(category))
            {
                success &= PowerMap.Compensate(category, targetWatt, out compensatedWatt);
                if (!success)
                    return false;
            }
            lock (SyncRoot)
            {
                double percentage = compensatedWatt / this.MaxPowerWatt * 100.0;
                if (percentage > 100)
                    percentage = 100;
                if (percentage < 0)
                    percentage = 0;
                switch (this.PowerControlMethod)
                {
                    default:
                        Logger.Log(Logger.Types.Error, $"laser [{this.Index}]: unsupported !");
                        return false;
                    case PowerControlMethods.DutyCycle:
                        double dutyCycle = this.MinDutyCycle + (this.MaxDutyCycle - this.MinDutyCycle) * percentage / 100.0;
                        double period = 1.0 / rtc.Frequency * (double)1.0e6; //usec
                        double tempPulseWidth = period * dutyCycle / 100.0;
                        success &= rtc.ListFrequency(rtc.Frequency, (double)tempPulseWidth);
                        success &= rtc.ListWait(this.PowerControlDelayTime);
                        break;
                }
            }
            if (success)
            {
                LastPowerWatt = targetWatt;
            }
            return success;
        }
        #endregion
    }
}
