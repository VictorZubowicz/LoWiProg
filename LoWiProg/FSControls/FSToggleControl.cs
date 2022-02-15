using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;



namespace LoWiProg.FSControls
{
    public class FSToggleButton : CheckBox
    {
        //Fields
        private Color onBackColor = Color.MediumSlateBlue;
        private Color onToggleColor = Color.WhiteSmoke;
        private Color offBackColor = Color.Gray;
        private Color offToggleColor = Color.Gainsboro;
        private bool solidStyle = true;

        //Propiedades creadas
        [Category("Appearance")]
        public Color OnBackColor { get { return onBackColor; } set { onBackColor = value; this.Invalidate(); } }

        [Category("Appearance")]
        public Color OnToggleColor { get => onToggleColor; set { onToggleColor = value; this.Invalidate(); } }

        [Category("Appearance")]
        public Color OffBackColor { get => offBackColor; set { offBackColor = value; this.Invalidate(); } }

        [Category("Appearance")]
        public Color OffToggleColor { get => offToggleColor; set { offToggleColor = value; this.Invalidate(); } }

        [Category("Appearance")]
        [DefaultValue(true)]
        public bool SolidStyle { get => solidStyle; set { solidStyle = value; this.Invalidate(); } }

        //Saco la propiedad Text de la lista
        public override string Text { get => base.Text; }


        //Constructor
        public FSToggleButton()
        {
            this.MinimumSize = new Size(30, 22);
        }

        private GraphicsPath GetFigurePath()
        {
            int arcSize = this.Height - 1;
            Rectangle leftArc = new Rectangle(0, 0, arcSize, arcSize);
            Rectangle rightArc = new Rectangle(this.Width - arcSize - 2, 0, arcSize, arcSize);

            GraphicsPath path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(leftArc, 90, 180);
            path.AddArc(rightArc, 270, 180);
            path.CloseFigure();
            return path;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            int toggleSize = this.Height - 5;
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            pevent.Graphics.Clear(this.Parent.BackColor);

            if (this.Checked)
            {
                //Draw the control surface
                if (SolidStyle)
                    pevent.Graphics.FillPath(new SolidBrush(onBackColor), GetFigurePath());
                else pevent.Graphics.DrawPath(new Pen(OnBackColor, 2), GetFigurePath());
                // Draw the toggle
                pevent.Graphics.FillEllipse(new SolidBrush(onToggleColor),
                    new Rectangle(this.Width - this.Height + 1, 2, toggleSize, toggleSize));
            }
            else //Off
            {
                //Draw the control surface
                if (SolidStyle)
                    pevent.Graphics.FillPath(new SolidBrush(offBackColor), GetFigurePath());
                else pevent.Graphics.DrawPath(new Pen(OffBackColor, 2), GetFigurePath());
                // Draw the toggle
                pevent.Graphics.FillEllipse(new SolidBrush(offToggleColor),
                    new Rectangle(2, 2, toggleSize, toggleSize));

            }
        }
    }
}
