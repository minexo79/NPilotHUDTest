using System.ComponentModel;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System;

namespace NPilotHUDTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // MAVLink
        MAVLink.MavlinkParse mavlink = new MAVLink.MavlinkParse();
        // locking to prevent multiple reads on serial port
        object readlock = new object();
        // our target sysid
        byte sysid;
        // our target compid
        byte compid;
        // MAVLink Serial Object
        SerialPort mavSerial = new SerialPort();

        BackgroundWorker bgw_HUD = new BackgroundWorker();
        BackgroundWorker bgw_MAV = new BackgroundWorker();

        public float RollState;
        public float PitchState;
        public float YawState;
        public float BetaState;
        public float VertState;
        public float GainState;
        public float AlphaState;
        public float AltitudeState;
        public float RollCommandState;

        public MainWindow()
        {
            InitializeComponent();           
            
            // reset hud state
            ResetHudState();

            bgw_HUD.DoWork += bgw_HUD_DoWork;
            bgw_HUD.RunWorkerCompleted += bgw_HUD_RunWorkerCompleted;
            bgw_HUD.RunWorkerAsync();

            //PreviewKeyDown += MainWindow_PreviewKeyDown;
            //PreviewKeyUp += MainWindow_PreviewKeyUp;

            //MouseDown += MainWindow_MouseDown1;

            cbPort.ItemsSource = SerialPort.GetPortNames();
        }

        #region Hide Area
        //private void MainWindow_MouseDown1(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.ChangedButton == MouseButton.Left)
        //    {
        //        try { DragMove(); } catch { }
        //    }
        //}

        //private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Escape)
        //    {
        //        Application.Current.Shutdown();
        //    }

        //    if (e.Key == Key.D)
        //    {
        //        RollState = 0;
        //    }
        //    if (e.Key == Key.A)
        //    {
        //        RollState = 0;
        //    }

        //    if (e.Key == Key.W)
        //    {
        //        PitchState = 0;
        //    }
        //    if (e.Key == Key.S)
        //    {
        //        PitchState = 0;
        //    }

        //    if (e.Key == Key.E)
        //    {
        //        YawState = 0;
        //    }
        //    if (e.Key == Key.Q)
        //    {
        //        YawState = 0;
        //    }

        //    if (e.Key == Key.Space)
        //    {
        //        VertGainState = 0;
        //        AltitudeState = 0;
        //    }
        //    if (e.Key == Key.LeftCtrl)
        //    {
        //        VertGainState = 0;
        //        AltitudeState = 0;
        //    }

        //    if (e.Key == Key.C)
        //    {
        //        AlphaState = 0;
        //    }
        //    if (e.Key == Key.Z)
        //    {
        //        AlphaState = 0;
        //    }
        //}

        //private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.R)
        //    {
        //        RollState = 0;
        //        PitchState = 0;
        //        YawState = 0;
        //        BetaState = 0;
        //        VertGainState = 0;
        //        AlphaState = 0;
        //        AltitudeState = 0;
        //        RollCommandState = 0;

        //        Hud_1.RollAngle = 0;
        //        Hud_1.PitchAngle = 0;
        //        Hud_1.YawAngle = 0;
        //        Hud_1.Beta = 0;
        //        Hud_1.VerticalSpeed = 0;
        //        Hud_1.Alpha = 0;
        //        Hud_1.Altitude = 0;
        //        Hud_1.RollCommand = 0;
        //        Hud_1.GroundSpeed = 0;
        //    }

        //    if (e.Key == Key.D)
        //    {
        //        RollState = 0.25f;
        //    }
        //    if (e.Key == Key.A)
        //    {
        //        RollState = -0.25f;
        //    }

        //    if (e.Key == Key.W)
        //    {
        //        PitchState = -0.25f;
        //    }
        //    if (e.Key == Key.S)
        //    {
        //        PitchState = 0.25f;
        //    }

        //    if (e.Key == Key.E)
        //    {
        //        YawState = 0.05f;
        //    }
        //    if (e.Key == Key.Q)
        //    {
        //        YawState = -0.05f;
        //    }

        //    if (e.Key == Key.Space)
        //    {
        //        VertGainState = 0.2f;
        //    }
        //    if (e.Key == Key.LeftCtrl)
        //    {
        //        VertGainState = -0.2f;
        //    }

        //    if (e.Key == Key.C)
        //    {
        //        AlphaState = 0.25f;
        //    }
        //    if (e.Key == Key.Z)
        //    {
        //        AlphaState = -0.25f;
        //    }
        //}
        #endregion

        private void bgw_HUD_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Hud_1.RollAngle     -= Hud_1.RollAngle;
            Hud_1.PitchAngle    -= Hud_1.PitchAngle;
            Hud_1.YawAngle      -= Hud_1.YawAngle;
            Hud_1.GroundSpeed   -= Hud_1.GroundSpeed;
            Hud_1.VerticalSpeed -= Hud_1.VerticalSpeed;
            Hud_1.Altitude      -= Hud_1.Altitude;

            Hud_1.RollAngle     += RollState;
            Hud_1.PitchAngle    += PitchState;
            Hud_1.YawAngle      += YawState;
            Hud_1.GroundSpeed   += GainState;
            Hud_1.VerticalSpeed += VertState;
            Hud_1.Altitude      += AltitudeState;

            Hud_1.Alpha         = AlphaState;
            Hud_1.RollCommand   = RollCommandState;
            bgw_HUD.RunWorkerAsync();
        }

        private void bgw_HUD_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1);
        }

        private void bgw_MAV_DoWork(object sender, DoWorkEventArgs e)
        {
            MAVLink.MAVLinkMessage packet;
            uint datastream_count = 0;
            MAVLink.MAV_DATA_STREAM[] datastream = new MAVLink.MAV_DATA_STREAM[] 
            {
                MAVLink.MAV_DATA_STREAM.POSITION,
                MAVLink.MAV_DATA_STREAM.EXTRA1,
                MAVLink.MAV_DATA_STREAM.EXTRA2,
            };

            while (mavSerial.IsOpen)
            {
                try
                {
                    lock (readlock)
                    {
                        // read any valid packet from the port
                        packet = mavlink.ReadPacket(mavSerial.BaseStream);

                        // check its valid
                        if (packet == null || packet.data == null)
                        {
                            Console.WriteLine("Non Packet!!!");
                        }
                    }

                    // check to see if its a hb packet from the comport
                    if (packet.data.GetType() == typeof(MAVLink.mavlink_heartbeat_t))
                    {
                        var hb = (MAVLink.mavlink_heartbeat_t)packet.data;

                        // save the sysid and compid of the seen MAV
                        sysid = packet.sysid;
                        compid = packet.compid;

                        if (datastream_count < 3)
                        {
                            SendDataStream(datastream[datastream_count], hb);
                            datastream_count += 1;
                        }

                        // Send Back Heartbeat Message
                        var buffer = mavlink.GenerateMAVLinkPacket10(MAVLink.MAVLINK_MSG_ID.HEARTBEAT, hb);

                        mavSerial.Write(buffer, 0, buffer.Length);
                    }

                    // from here we should check the the message is addressed to us
                    if (sysid == packet.sysid || compid == packet.compid)
                    {
                        // Console.WriteLine(packet.msgtypename);

                        if (packet.msgid == (byte)MAVLink.MAVLINK_MSG_ID.ATTITUDE)
                        {
                            var att = (MAVLink.mavlink_attitude_t)packet.data;

                            PitchState      = att.pitch * (float)57.2958;
                            RollState       = att.roll * (float)57.2958;
                            YawState        = att.yaw * (float)57.2958; 
                        }

                        else if (packet.msgid == (byte)MAVLink.MAVLINK_MSG_ID.VFR_HUD)
                        {
                            var vfr         = (MAVLink.mavlink_vfr_hud_t)packet.data;
                            GainState       = vfr.groundspeed;
                        }

                        else if (packet.msgid == (byte)MAVLink.MAVLINK_MSG_ID.GLOBAL_POSITION_INT)
                        {
                            var gps         = (MAVLink.mavlink_global_position_int_t)packet.data;
                            VertState       = (float)gps.vz / 100;
                            AltitudeState   = (float)gps.relative_alt / 1000;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"> ERROR! {ex.Message}");
                }
                finally
                {
                    Thread.Sleep(1);
                }
            }
        }

        private void SendDataStream(MAVLink.MAV_DATA_STREAM data_stream, object hb)
        {
            // request EXTRA1 streams at 5 hz
            var buffer = mavlink.GenerateMAVLinkPacket10(MAVLink.MAVLINK_MSG_ID.REQUEST_DATA_STREAM,
                new MAVLink.mavlink_request_data_stream_t()
                {
                    req_message_rate = 5,
                    req_stream_id = (byte)data_stream,
                    start_stop = 1,
                    target_component = compid,
                    target_system = sysid
                });

            mavSerial.Write(buffer, 0, buffer.Length);
        }

        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (mavSerial.IsOpen)
            {
                // reset hud state
                ResetHudState();

                mavSerial.Close(); return;
            }

            // set the comport options
            mavSerial.PortName = cbPort.Text;
            mavSerial.BaudRate = int.Parse(cbBaud.Text);

            // set Read Timeout
            mavSerial.ReadTimeout = 5000;

            // open Comport
            mavSerial.Open();

            // Initialize mav read bgw
            bgw_MAV.DoWork += bgw_MAV_DoWork;
            bgw_MAV.RunWorkerAsync();
        }

        private void ResetHudState()
        {
            RollState = 0;
            PitchState = 0;
            YawState = 0;
            BetaState = 0;

            GainState = 0;
            VertState = 0;
            AltitudeState = 0;
        }
    }
}
