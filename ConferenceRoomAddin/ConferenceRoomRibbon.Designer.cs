namespace ConferenceRoomAddin
{
    partial class ConferenceRoomRibbon : Microsoft.Office.Tools.Ribbon.RibbonBase
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        public ConferenceRoomRibbon()
            : base(Globals.Factory.GetRibbonFactory())
        {
            InitializeComponent();
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabCR = this.Factory.CreateRibbonTab();
            this.groupCRS = this.Factory.CreateRibbonGroup();
            this.ReserveButton = this.Factory.CreateRibbonButton();
            this.ModifyButton = this.Factory.CreateRibbonButton();
            this.tabCR.SuspendLayout();
            this.groupCRS.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabCR
            // 
            this.tabCR.ControlId.ControlIdType = Microsoft.Office.Tools.Ribbon.RibbonControlIdType.Office;
            this.tabCR.ControlId.OfficeId = "TabAppointment";
            this.tabCR.Groups.Add(this.groupCRS);
            this.tabCR.Label = "TabAppointment";
            this.tabCR.Name = "tabCR";
            // 
            // groupCRS
            // 
            this.groupCRS.Items.Add(this.ReserveButton);
            this.groupCRS.Items.Add(this.ModifyButton);
            this.groupCRS.Label = "MRBS";
            this.groupCRS.Name = "groupCRS";
            // 
            // ReserveButton
            // 
            this.ReserveButton.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.ReserveButton.Image = global::ConferenceRoomAddin.Properties.Resources.MJLogo;
            this.ReserveButton.Label = "Reserve a Conference Room";
            this.ReserveButton.Name = "ReserveButton";
            this.ReserveButton.ShowImage = true;
            this.ReserveButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.ReserveButton_Click);
            // 
            // ModifyButton
            // 
            this.ModifyButton.ControlSize = Microsoft.Office.Core.RibbonControlSize.RibbonControlSizeLarge;
            this.ModifyButton.Image = global::ConferenceRoomAddin.Properties.Resources.MJLogo;
            this.ModifyButton.Label = "Modify Reservation";
            this.ModifyButton.Name = "ModifyButton";
            this.ModifyButton.ShowImage = true;
            this.ModifyButton.Click += new Microsoft.Office.Tools.Ribbon.RibbonControlEventHandler(this.ModifyButton_Click);
            // 
            // ConferenceRoomRibbon
            // 
            this.Name = "ConferenceRoomRibbon";
            this.RibbonType = "Microsoft.Outlook.Appointment";
            this.Tabs.Add(this.tabCR);
            this.Close += new System.EventHandler(this.ConferenceRoomRibbon_Close);
            this.Load += new Microsoft.Office.Tools.Ribbon.RibbonUIEventHandler(this.ConferenceRoomRibbon_Load);
            this.tabCR.ResumeLayout(false);
            this.tabCR.PerformLayout();
            this.groupCRS.ResumeLayout(false);
            this.groupCRS.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        internal Microsoft.Office.Tools.Ribbon.RibbonTab tabCR;
        internal Microsoft.Office.Tools.Ribbon.RibbonGroup groupCRS;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton ReserveButton;
        internal Microsoft.Office.Tools.Ribbon.RibbonButton ModifyButton;
    }

    partial class ThisRibbonCollection
    {
        internal ConferenceRoomRibbon ConferenceRoomRibbon
        {
            get { return this.GetRibbon<ConferenceRoomRibbon>(); }
        }
    }
}
