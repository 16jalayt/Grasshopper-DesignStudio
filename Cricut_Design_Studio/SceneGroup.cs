using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using static Cricut_Design_Studio.SceneGroup;

namespace Cricut_Design_Studio
{
    public class SceneGroup : SceneNode
    {
        public class CutCartRecord
        {
            private static ArrayList cutCartRecords = new ArrayList();

            private static bool showTrialWarning = true;

            private static bool isReset = true;

            public string fontName;

            public bool skipped = true;

            public CutCartRecord(string name)
            {
                fontName = name;
            }

            public static int nActive()
            {
                int num = 0;
                foreach (CutCartRecord cutCartRecord in cutCartRecords)
                {
                    if (!cutCartRecord.skipped)
                    {
                        num++;
                    }
                }
                return num;
            }

            public static void reset()
            {
                cutCartRecords.Clear();
                showTrialWarning = true;
                isReset = true;
            }

            public static CutCartRecord find(string fontName, PcControl pc)
            {
                if (fontName == null || fontName.Length < 5)
                {
                    return null;
                }
                if (fontName.CompareTo("Makin the Grade") == 0)
                {
                    fontName = "Makin' the Grade";
                }
                if (isReset)
                {
                    /*Form1.myRootForm.messageBoxOpen = true;
                    DialogResult dialogResult = MessageBox.Show(Form1.myRootForm, "Click \"Yes\" ONLY if you have a Cricut Jukebox connected to your Cricut machine\nand would like to check for cartridges automatically. (This check will take a few moments.)\n\nOtherwise, please click \"No\" to check for cartridges manually.", "Cricut Jukebox Check", MessageBoxButtons.YesNo);
                    Form1.myRootForm.messageBoxOpen = false;
                    if (dialogResult == DialogResult.Yes)
                    {
                        ArrayList arrayList = pc.readJukeboxes(justSelectFirst: false);
                        if (arrayList != null)
                        {
                            foreach (string item in arrayList)
                            {
                                if (item != null && item.Length > 0)
                                {
                                    CutCartRecord cutCartRecord = new CutCartRecord(item);
                                    cutCartRecord.skipped = false;
                                    cutCartRecords.Add(cutCartRecord);
                                }
                            }
                        }
                        pc.readJukeboxes(justSelectFirst: true);
                    }*/
                    isReset = false;
                }
                //cutCartRecords are currently inserted in machine
                //fontName is requested font
                foreach (CutCartRecord cutCartRecord2 in cutCartRecords)
                {
                    if (fontName.CompareTo(cutCartRecord2.fontName) == 0)
                    {
                        return cutCartRecord2;
                    }
                }
                return null;
            }

            public static void add(SceneGroup sg, PcControl pc, string fontName)
            {
                /*CutCartRecord cutCartRecord = new CutCartRecord(fontName);
                cutCartRecord.skipped = true;
                if (PcCache.cricutModel != 0)
                {
                    if ((Form1.myRootForm.trialMode || PcCache.trialMode) && !sg.isTrialCartridge(fontName))
                    {
                        if (showTrialWarning)
                        {
                            sg.showTrialWarning();
                            showTrialWarning = false;
                        }
                    }
                    else if (sg.checkCartridge(pc, fontName))
                    {
                        cutCartRecord.skipped = false;
                    }
                    else if (sg.askForInsertCartridge(fontName) && sg.checkCartridge(pc, fontName))
                    {
                        cutCartRecord.skipped = false;
                    }
                }
                cutCartRecords.Add(cutCartRecord);*/

                CutCartRecord cutCartRecord = new CutCartRecord(fontName);
                cutCartRecord.skipped = false;
                cutCartRecords.Add(cutCartRecord);
            }
        }

        public class Transformers
        {
            public float mlen;

            public float hlen;

            public float nlen;

            public PointF mvec;

            public PointF hvec;

            public PointF nvec;

            public float dot;

            public float ndot;

            public PointF anchor;

            public void calc(PointF org, PointF mouse, PointF hot, Matrix modelingTransform)
            {
                PointF[] array = new PointF[1] { org };
                modelingTransform.TransformPoints(array);
                anchor = array[0];
                mvec = new PointF(mouse.X - anchor.X, mouse.Y - anchor.Y);
                mlen = (float)Math.Sqrt(mvec.X * mvec.X + mvec.Y * mvec.Y);
                hvec = new PointF(hot.X - anchor.X, hot.Y - anchor.Y);
                hlen = (float)Math.Sqrt(hvec.X * hvec.X + hvec.Y * hvec.Y);
                if (Math.Abs(mlen) > float.Epsilon && Math.Abs(hlen) > float.Epsilon)
                {
                    dot = mvec.X / mlen * (hvec.X / hlen) + mvec.Y / mlen * (hvec.Y / hlen);
                }
                else
                {
                    dot = 0f;
                }
                nvec = new PointF(mouse.X - hot.X, mouse.Y - hot.Y);
                nlen = (float)Math.Sqrt(nvec.X * nvec.X + nvec.Y * nvec.Y);
                if (Math.Abs(nlen) > float.Epsilon && Math.Abs(hlen) > float.Epsilon)
                {
                    ndot = nvec.X / nlen * (hvec.X / hlen) + nvec.Y / nlen * (hvec.Y / hlen);
                }
                else
                {
                    ndot = 0f;
                }
            }
        }

        public enum DragRelativeTo
        {
            Self,
            PrimarySelection
        }

        public static ArrayList localFontNames = new ArrayList();

        public static SceneGroup copyBuffer = null;

        public float oldShear;

        public float shear;

        public Matrix transform = new Matrix();

        public Matrix otm = new Matrix();

        public RectangleF bbox = new RectangleF(0f, 0f, 0f, 0f);

        public float baseline;

        private static ulong selectionCount = 0uL;

        public ulong selectedCount;

        public bool tentativelySelected;

        public FloatParam erosion = new FloatParam(0f, 0);

        public FloatParam spacing = new FloatParam(0f, 0);

        public bool flipShapes;

        public bool welding;

        public bool negativeWeld;

        public bool kerning;

        public bool dirty;

        public static bool hideTransformHandles = false;

        public static bool dragging = false;

        public static PointF mousePnt;

        public static PointF wmousePnt;

        public bool renderType;

        private static string lastStringRead = null;

        public bool selected
        {
            get
            {
                return selectedCount != 0;
            }
            set
            {
                if (value)
                {
                    selectedCount = ++selectionCount;
                    if (selectionCount == 0)
                    {
                        selectedCount = ++selectionCount;
                    }
                }
                else
                {
                    selectedCount = 0uL;
                }
            }
        }

        public bool lastSelected => selectedCount == selectionCount;

        public string SelectionUndoIdentity
        {
            get
            {
                string text = transform.ToString();
                foreach (SceneNode child in children)
                {
                    SceneGlyph sceneGlyph = (SceneGlyph)child;
                    if (sceneGlyph != null)
                    {
                        object obj = text;
                        text = string.Concat(obj, " (", sceneGlyph.fontId, " ", sceneGlyph.keyId, ")");
                    }
                }
                return text;
            }
        }

        public SceneGroup()
        {
            erosion.parent = this;
            spacing.parent = this;
        }

        public void moveGroup(float dx, float dy)
        {
            dirty = true;
            transform.Translate(dx, dy, MatrixOrder.Append);
        }

        public static void moveGroups(SceneGroup[] groups, float dx, float dy)
        {
            if (groups.Length > 0)
            {
                foreach (SceneGroup sceneGroup in groups)
                {
                    sceneGroup.moveGroup(dx, dy);
                }
                SceneGroup[] psg = CopySceneGroups(groups);
                Form1.myRootForm.getCanvas().undoRedo.add(new UndoRedo.UndoTransformShapes(psg, groups));
            }
        }

        public static SceneGroup[] CopySceneGroups(SceneGroup[] groups)
        {
            SceneGroup[] array = new SceneGroup[groups.Length];
            for (int i = 0; i < groups.Length; i++)
            {
                array[i] = (SceneGroup)groups[i].getCopy();
            }
            return array;
        }

        public void saveTransform()
        {
            oldShear = shear;
            otm = transform.Clone();
        }

        public void resetTransform()
        {
            dirty = true;
            shear = 0f;
            transform.Reset();
            otm.Reset();
        }

        public Matrix getTransform()
        {
            Matrix matrix = new Matrix();
            matrix.Translate(0f - bbox.Left, 0f - bbox.Bottom, MatrixOrder.Append);
            matrix.Shear(0f - shear, 0f, MatrixOrder.Append);
            matrix.Translate(bbox.Left, bbox.Bottom, MatrixOrder.Append);
            matrix.Multiply(transform, MatrixOrder.Append);
            return matrix;
        }

        public void kern(Graphics g)
        {
            base.draw(g);
        }

        public override void draw(Graphics g)
        {
            Matrix matrix = g.Transform.Clone();
            Matrix matrix2 = g.Transform;
            matrix2.Multiply(getTransform(), MatrixOrder.Prepend);
            try
            {
                g.Transform = matrix2;
            }
            catch
            {
                g.Transform = matrix;
            }
            base.draw(g);
            g.Transform = matrix;
            if (children.Count <= 0 || (Canvas.drawMode & 0x10) == 0)
            {
                return;
            }
            Pen pen = new Pen(Color.FromArgb(64, 0, 0, 0), Canvas.penScale);
            Matrix modelingTransform = new Matrix();
            PointF pointF = new PointF(0f, 0f);
            PointF[] array = new PointF[4]
            {
                new PointF(bbox.Left, bbox.Bottom),
                new PointF(bbox.Right, bbox.Bottom),
                new PointF(bbox.Right, bbox.Top),
                new PointF(bbox.Left, bbox.Top)
            };
            getTransform().TransformPoints(array);
            for (int i = 0; i < array.Length; i++)
            {
                pointF.X += array[i].X;
                pointF.Y += array[i].Y;
            }
            pointF.X /= array.Length;
            pointF.Y /= array.Length;
            g.DrawPolygon(pen, array);
            g.DrawLine(pen, pointF.X - 16f * Canvas.penScale, pointF.Y, pointF.X + 16f * Canvas.penScale, pointF.Y);
            g.DrawLine(pen, pointF.X, pointF.Y - 16f * Canvas.penScale, pointF.X, pointF.Y + 16f * Canvas.penScale);
            if ((selected || tentativelySelected) && !hideTransformHandles)
            {
                float siz = (float)Math.Sqrt(bbox.Width * bbox.Width + bbox.Height * bbox.Height) / 7f;
                PointF cm = new PointF((bbox.Left + bbox.Right) / 2f, (bbox.Bottom + bbox.Top) / 2f);
                if (lastSelected)
                {
                    SceneUtils.drawHandles(g, array, cm, siz, dragging, wmousePnt, modelingTransform, 510);
                }
                else
                {
                    SceneUtils.drawSelected(g, array, cm, siz, dragging, wmousePnt, modelingTransform, 510, tentativelySelected);
                }
                updateNumBoxes();
            }
        }

        private bool isTrialCartridge(string fontName)
        {
            if (10 == PcCache.cricutModel && fontName.CompareTo("George and Basic Shapes") == 0)
            {
                return true;
            }
            if (20 == PcCache.cricutModel && (fontName.CompareTo("Plantin Schoolbook") == 0 || fontName.CompareTo("Accent Essentials") == 0))
            {
                return true;
            }
            if (15 == PcCache.cricutModel && fontName.CompareTo("DonJuan") == 0)
            {
                return true;
            }
            return false;
        }

        private bool checkCartridge(PcControl pc, string fontName)
        {
            int cartIsProgrammed = 0;
            string cartridge = pc.getCartridge(ref cartIsProgrammed);
            if (cartIsProgrammed != 0 && cartridge.CompareTo(fontName) == 0)
            {
                return true;
            }
            return false;
        }

        private void showTrialWarning()
        {
            string text = "";
            string text2 = "";
            switch (PcCache.cricutModel)
            {
                case 10:
                    text = "George and Basic Shapes";
                    text2 = "Cricut Personal Cutter";
                    break;

                case 20:
                    text = "Plantin Schoolbook and Accent Essentials";
                    text2 = "Cricut Expression";
                    break;

                case 15:
                    text = "DonJuan";
                    text2 = "Cricut Create";
                    break;
            }
            Form1.myRootForm.messageBoxOpen = true;
            MessageBox.Show(Form1.myRootForm, "Some cartridges won't cut in trial mode.\n\nOne or more of the catridges used in this project will not cut because this copy of Cricut DesignStudio is running in trial mode.\nIn trial mode, only " + text + " will cut using your " + text2 + ".", "Trial Mode", MessageBoxButtons.OK);
            Form1.myRootForm.messageBoxOpen = false;
        }

        private bool askForInsertCartridge(string fontName)
        {
            Form1.myRootForm.messageBoxOpen = true;
            DialogResult dialogResult = MessageBox.Show(Form1.myRootForm, "Please insert the\"" + fontName + "\" cartridge into the Cricut and click OK to continue cutting,\nor click Cancel to skip cutting the shapes that use this cartridge.\n\n(Your choice will be remembered for this project for the rest of this session.)", "Insert Cartridge", MessageBoxButtons.OKCancel);
            Form1.myRootForm.messageBoxOpen = false;
            switch (dialogResult)
            {
                case DialogResult.OK:
                    return true;

                case DialogResult.Cancel:
                    return false;

                default:
                    return false;
            }
        }

        public void cut(PcControl pc, Matrix m)
        {
            Matrix matrix = m.Clone();
            long num = 0L;
            int num2 = (int)(num & 0xFFFFFFFFu);
            int num3 = (int)((num >> 32) & 0xFFFFFFFFu);
            _ = "(" + num3 + "/" + num2 + ") ";
            Thread.Sleep(250);
            int cartIsProgrammed = 0;
            //Get current cartridge name from printer
            string cartridge = pc.getCartridge(ref cartIsProgrammed);
            if (cartIsProgrammed != 0 && CutCartRecord.find(cartridge, pc) == null)
            {
                CutCartRecord.add(this, pc, cartridge);
            }
            //Loop objects in scene to make sure fonts loaded
            foreach (SceneNode child in children)
            {
                //child.shape.header.cartHeader.fontName
                SceneGlyph sceneGlyph = (SceneGlyph)child;
                if (sceneGlyph.shape != null)
                {
                    string fontName = sceneGlyph.shape.getFontName();
                    CutCartRecord cutCartRecord = CutCartRecord.find(fontName, pc);
                    if (cutCartRecord == null)
                    {
                        CutCartRecord.add(this, pc, fontName);
                    }
                }
            }
            if (CutCartRecord.nActive() > 18)
            {
                CutCartRecord.reset();
                Form1.myRootForm.messageBoxOpen = true;
                MessageBox.Show(Form1.myRootForm, "Sorry, but the Cricut DesignStudio can only handle 18 or fewer cartridges at one time.\n\nPlease limit the number of cartridges used on the current page to 18 and then try your cut again.", "Too Many Cartridges", MessageBoxButtons.OK);
                Form1.myRootForm.messageBoxOpen = false;
            }
            matrix.Multiply(getTransform(), MatrixOrder.Prepend);
            foreach (SceneNode child2 in children)
            {
                SceneGlyph sceneGlyph2 = (SceneGlyph)child2;
                if (sceneGlyph2.shape != null)
                {
                    string fontName2 = sceneGlyph2.shape.getFontName();
                    CutCartRecord cutCartRecord2 = CutCartRecord.find(fontName2, pc);
                    if (cutCartRecord2 != null && !cutCartRecord2.skipped)
                    {
                        sceneGlyph2.parent = this;
                        sceneGlyph2.cut(pc, matrix);
                    }
                }
            }
        }

        public override bool wouldSelect(PointF mpnt, Matrix t)
        {
            Matrix matrix = t.Clone();
            matrix.Multiply(getTransform(), MatrixOrder.Prepend);
            return base.wouldSelect(mpnt, matrix);
        }

        public override bool select(PointF mpnt, Matrix t)
        {
            Matrix matrix = t.Clone();
            matrix.Multiply(getTransform(), MatrixOrder.Prepend);
            return selected = base.select(mpnt, matrix);
        }

        public override void saveGypsy(GypsyWriter gw, int layerNo)
        {
            float tx = 0f;
            float ty = 0f;
            float len = 0f;
            float len2 = 0f;
            float angle = 0f;
            float xshear = 0f;
            getTransformValues(ref tx, ref ty, ref len, ref len2, ref angle, ref xshear);
            gw.transform = transform.Clone();
            gw.shear = shear;
            gw.angle = angle;
            gw.bbox = bbox;
            gw.sg = this;
            gw.WriteGroupHeader(0f, xshear, 0f, gw.angle, layerNo, flipShapes);
            foreach (SceneNode child in children)
            {
                if (typeof(SceneGlyph) == child.GetType() && ((SceneGlyph)child).shape != null)
                {
                    child.saveGypsy(gw, layerNo);
                }
            }
            gw.EndGroupHeader();
            calcBbox(null);
        }

        public override void save(BinaryWriter bw)
        {
            Matrix matrix = getTransform().Clone();
            bw.Write("SceneGroup");
            for (int i = 0; i < matrix.Elements.Length; i++)
            {
                bw.Write(matrix.Elements[i]);
            }
            bw.Write(erosion.f);
            bw.Write(spacing.f);
            bw.Write(flipShapes);
            bw.Write(welding);
            localFontNames.Clear();
            foreach (SceneNode child in children)
            {
                if (typeof(SceneGlyph) != child.GetType() || ((SceneGlyph)child).shape == null)
                {
                    continue;
                }
                string fontName = ((SceneGlyph)child).shape.header.cartHeader.fontName;
                int num = -1;
                int num2 = 0;
                foreach (string localFontName in localFontNames)
                {
                    if (localFontName.CompareTo(fontName) == 0)
                    {
                        num = num2;
                        break;
                    }
                    num2++;
                }
                if (-1 == num)
                {
                    localFontNames.Add(fontName);
                    num = localFontNames.Count - 1;
                }
                ((SceneGlyph)child).localFontId = num;
            }
            bw.Write(localFontNames.Count);
            foreach (string localFontName2 in localFontNames)
            {
                bw.Write(localFontName2);
            }
            bw.Write(children.Count);
            foreach (SceneNode child2 in children)
            {
                child2.save(bw);
            }
            localFontNames.Clear();
            dirty = false;
        }

        public static SceneNode read(BinaryReader br)
        {
            SceneGroup sceneGroup = new SceneGroup();
            float[] array = new float[6];
            float num = 0f;
            for (int i = 0; i < sceneGroup.transform.Elements.Length; i++)
            {
                array[i] = br.ReadSingle();
            }
            sceneGroup.transform = new Matrix(array[0], array[1], array[2], array[3], array[4], array[5]);
            sceneGroup.erosion.f = br.ReadSingle();
            sceneGroup.spacing.f = br.ReadSingle();
            sceneGroup.flipShapes = br.ReadBoolean();
            sceneGroup.welding = br.ReadBoolean();
            localFontNames.Clear();
            int num2 = br.ReadInt32();
            for (int j = 0; j < num2; j++)
            {
                string value = br.ReadString();
                localFontNames.Add(value);
            }
            num2 = br.ReadInt32();
            for (int k = 0; k < num2; k++)
            {
                string text = readNextString(br, "SceneGlyph");
                if (text != null)
                {
                    SceneGlyph child = (SceneGlyph)SceneGlyph.read(br);
                    sceneGroup.add(child, num);
                }
            }
            return sceneGroup;
        }

        public static string readNextString(BinaryReader br, string match)
        {
            string text = null;
            text = ((lastStringRead == null) ? br.ReadString() : new string(lastStringRead.ToCharArray()));
            if (match == null || string.Compare(text, match) == 0)
            {
                lastStringRead = null;
                return text;
            }
            lastStringRead = new string(text.ToCharArray());
            return null;
        }

        public override void listCarts(ArrayList fontNames, ArrayList shapes)
        {
            foreach (SceneNode child in children)
            {
                if (typeof(SceneGlyph) != child.GetType() || ((SceneGlyph)child).shape == null)
                {
                    continue;
                }
                string fontName = ((SceneGlyph)child).shape.header.cartHeader.fontName;
                int num = -1;
                int num2 = 0;
                foreach (string fontName2 in fontNames)
                {
                    if (fontName2.CompareTo(fontName) == 0)
                    {
                        num = num2;
                        break;
                    }
                    num2++;
                }
                if (-1 == num)
                {
                    fontNames.Add(fontName);
                    shapes.Add(((SceneGlyph)child).shape);
                }
            }
        }

        public SceneNode getCopy()
        {
            SceneGroup sceneGroup = new SceneGroup();
            sceneGroup.oldShear = oldShear;
            sceneGroup.otm = otm.Clone();
            sceneGroup.renderType = renderType;
            sceneGroup.bbox = bbox;
            sceneGroup.transform = transform.Clone();
            sceneGroup.erosion.f = erosion.f;
            sceneGroup.spacing.f = spacing.f;
            sceneGroup.baseline = baseline;
            sceneGroup.flipShapes = flipShapes;
            sceneGroup.welding = welding;
            sceneGroup.shear = shear;
            foreach (SceneGlyph child in children)
            {
                sceneGroup.add(child.getCopy(), sceneGroup.baseline);
            }
            return sceneGroup;
        }

        public void add(SceneNode child, float baseline)
        {
            dirty = true;
            if (children.Count < 1)
            {
                this.baseline = baseline;
            }
            children.Add(child);
            child.parent = this;
        }

        public SceneNode delLast()
        {
            if (children.Count == 0)
            {
                return null;
            }
            dirty = true;
            children.RemoveAt(children.Count - 1);
            calcBbox(null);
            if (children.Count == 0)
            {
                return null;
            }
            return (SceneNode)children[children.Count - 1];
        }

        public void calcBbox(SceneGlyph garg)
        {
            float num = float.MaxValue;
            float num2 = float.MaxValue;
            float num3 = float.MinValue;
            float num4 = float.MinValue;
            foreach (SceneGlyph child in children)
            {
                if (child.isSpace || (garg != null && garg != child))
                {
                    continue;
                }
                PointF[] array = new PointF[4]
                {
                    new PointF(child.bbox.X, child.bbox.Y),
                    new PointF(child.bbox.X, child.bbox.Y + child.bbox.Height),
                    new PointF(child.bbox.X + child.bbox.Width, child.bbox.Y + child.bbox.Height),
                    new PointF(child.bbox.X + child.bbox.Width, child.bbox.Y)
                };
                child.glyphToWorld.TransformPoints(array);
                PointF[] array2 = array;
                for (int i = 0; i < array2.Length; i++)
                {
                    PointF pointF = array2[i];
                    if (pointF.X < num)
                    {
                        num = pointF.X;
                    }
                    if (pointF.Y < num2)
                    {
                        num2 = pointF.Y;
                    }
                    if (pointF.X > num3)
                    {
                        num3 = pointF.X;
                    }
                    if (pointF.Y > num4)
                    {
                        num4 = pointF.Y;
                    }
                }
            }
            bbox.Location = new PointF(num, num2);
            bbox.Size = new SizeF(num3 - num, num4 - num2);
        }

        public static void clearNumBoxes()
        {
            Form1.myRootForm.shearTextBox.Text = "0";
            Form1.myRootForm.shearTextBox.Enabled = false;
            Form1.myRootForm.xPositionTextBox.Text = "0";
            Form1.myRootForm.xPositionTextBox.Enabled = false;
            Form1.myRootForm.yPositionTextBox.Text = "0";
            Form1.myRootForm.yPositionTextBox.Enabled = false;
            Form1.myRootForm.widthTextBox.Text = "0";
            Form1.myRootForm.widthTextBox.Enabled = false;
            Form1.myRootForm.heightTextBox.Text = "0";
            Form1.myRootForm.heightTextBox.Enabled = false;
            Form1.myRootForm.angleTextBox.Text = "0";
            Form1.myRootForm.angleTextBox.Enabled = false;
            Form1.myRootForm.flipShapesCheckBox.Checked = false;
            Form1.myRootForm.flipShapesCheckBox.Enabled = false;
            Form1.myRootForm.weldingCheckBox.Checked = false;
            Form1.myRootForm.weldingCheckBox.Enabled = false;
        }

        public void getTransformValues(ref float tx, ref float ty, ref float len1, ref float len2, ref float angle, ref float shear)
        {
            shear = this.shear;
            Matrix matrix = transform.Clone();
            PointF[] array = new PointF[3]
            {
                new PointF(bbox.X, bbox.Y),
                new PointF(bbox.X + bbox.Width, bbox.Y),
                new PointF(bbox.X, bbox.Y + bbox.Height)
            };
            matrix.TransformPoints(array);
            float num = array[1].X - array[0].X;
            float num2 = array[1].Y - array[0].Y;
            float num3 = array[2].X - array[0].X;
            float num4 = array[2].Y - array[0].Y;
            float num5 = 0f;
            tx = array[0].X;
            ty = array[0].Y;
            len1 = (float)Math.Sqrt(num * num + num2 * num2);
            len2 = (float)Math.Sqrt(num3 * num3 + num4 * num4);
            angle = 0f;
            if (Math.Abs(len1) > float.Epsilon)
            {
                num5 = num / len1;
                if (num5 < -1f || num5 > 1f)
                {
                    Console.WriteLine("dot error");
                    if (num5 < -1f)
                    {
                        num5 = -1f;
                    }
                    if (num5 > 1f)
                    {
                        num5 = 1f;
                    }
                }
                angle = (float)Math.Acos(num5) * 180f / (float)Math.PI;
                if (num2 < 0f)
                {
                    angle = 0f - angle;
                }
            }
            else
            {
                if (!(Math.Abs(len2) > float.Epsilon))
                {
                    return;
                }
                num5 = num4 / len2;
                if (num5 < -1f || num5 > 1f)
                {
                    Console.WriteLine("dot error");
                    if (num5 < -1f)
                    {
                        num5 = -1f;
                    }
                    if (num5 > 1f)
                    {
                        num5 = 1f;
                    }
                }
                angle = (float)Math.Acos(num5) * 180f / (float)Math.PI;
                if (num3 < 0f)
                {
                    angle = 0f - angle;
                }
            }
        }

        public void updateNumBoxes()
        {
            if (children.Count >= 1)
            {
                string text = "#.###;-#.###;0";
                float tx = 0f;
                float ty = 0f;
                float len = 0f;
                float len2 = 0f;
                float angle = 0f;
                float num = 0f;
                getTransformValues(ref tx, ref ty, ref len, ref len2, ref angle, ref num);
                ((FloatParam)Form1.myRootForm.xPositionTextBox.Tag).f = tx - 1f;
                Form1.myRootForm.xPositionTextBox.Text = (tx - 1f).ToString(text);
                Form1.myRootForm.xPositionTextBox.Enabled = true;
                ((FloatParam)Form1.myRootForm.yPositionTextBox.Tag).f = ty - 0.5f;
                Form1.myRootForm.yPositionTextBox.Text = (ty - 0.5f).ToString(text);
                Form1.myRootForm.yPositionTextBox.Enabled = true;
                ((FloatParam)Form1.myRootForm.widthTextBox.Tag).f = len;
                Form1.myRootForm.widthTextBox.Text = len.ToString(text);
                Form1.myRootForm.widthTextBox.Enabled = true;
                ((FloatParam)Form1.myRootForm.heightTextBox.Tag).f = len2;
                Form1.myRootForm.heightTextBox.Text = len2.ToString(text);
                Form1.myRootForm.heightTextBox.Enabled = true;
                ((FloatParam)Form1.myRootForm.angleTextBox.Tag).f = angle;
                Form1.myRootForm.angleTextBox.Text = angle.ToString(text);
                Form1.myRootForm.angleTextBox.Enabled = true;
                ((FloatParam)Form1.myRootForm.shearTextBox.Tag).f = 100f * shear;
                Form1.myRootForm.shearTextBox.Text = (100f * shear).ToString(text);
                Form1.myRootForm.shearTextBox.Enabled = true;
                Form1.myRootForm.flipShapesCheckBox.Enabled = true;
                Form1.myRootForm.flipShapesCheckBox.Checked = flipShapes;
                Form1.myRootForm.weldingCheckBox.Enabled = true;
                Form1.myRootForm.weldingCheckBox.Checked = welding;
            }
        }

        public static void doNumBoxTransform(int mode)
        {
            if (Form1.myRootForm == null || Form1.myRootForm.getCanvas() == null)
            {
                return;
            }
            SceneGroup[] selectedGroups = Form1.myRootForm.getCanvas().GetSelectedGroups();
            SceneGroup[] array = selectedGroups;
            foreach (SceneGroup sceneGroup in array)
            {
                if (sceneGroup == null || sceneGroup.children == null || sceneGroup.children.Count < 1)
                {
                    return;
                }
                float num = 0f;
                float tx = 0f;
                float ty = 0f;
                float len = 0f;
                float len2 = 0f;
                float angle = 0f;
                float num2 = 0f;
                sceneGroup.saveTransform();
                sceneGroup.getTransformValues(ref tx, ref ty, ref len, ref len2, ref angle, ref num2);
                PointF pointF = new PointF((sceneGroup.bbox.Left + sceneGroup.bbox.Right) / 2f, (sceneGroup.bbox.Top + sceneGroup.bbox.Bottom) / 2f);
                Matrix matrix = new Matrix();
                PointF[] array2 = new PointF[2]
                {
                    pointF,
                    new PointF(sceneGroup.bbox.Left, sceneGroup.bbox.Top)
                };
                sceneGroup.otm.TransformPoints(array2);
                _ = array2[0];
                float num3 = 0.5f;
                float num4 = 1f;
                switch (mode)
                {
                    case 2:
                        num = ((FloatParam)Form1.myRootForm.heightTextBox.Tag).f;
                        if (num < num4)
                        {
                            num = num4;
                            ((FloatParam)Form1.myRootForm.heightTextBox.Tag).f = num;
                        }
                        num /= len2;
                        matrix.Translate(0f - array2[1].X, 0f - array2[1].Y, MatrixOrder.Append);
                        matrix.Scale(1f, num, MatrixOrder.Append);
                        matrix.Translate(array2[1].X, array2[1].Y, MatrixOrder.Append);
                        break;

                    case 4:
                        num = ((FloatParam)Form1.myRootForm.widthTextBox.Tag).f;
                        if (num < num3)
                        {
                            num = num3;
                            ((FloatParam)Form1.myRootForm.widthTextBox.Tag).f = num;
                        }
                        num /= len;
                        matrix.Translate(0f - array2[1].X, 0f - array2[1].Y, MatrixOrder.Append);
                        matrix.Scale(num, 1f, MatrixOrder.Append);
                        matrix.Translate(array2[1].X, array2[1].Y, MatrixOrder.Append);
                        break;

                    case 6:
                        num = ((FloatParam)Form1.myRootForm.yPositionTextBox.Tag).f;
                        matrix.Translate(0f, num - ty + 0.5f, MatrixOrder.Append);
                        break;

                    case 8:
                        num = ((FloatParam)Form1.myRootForm.xPositionTextBox.Tag).f;
                        matrix.Translate(num - tx + 1f, 0f, MatrixOrder.Append);
                        break;

                    case 7:
                        num = ((FloatParam)Form1.myRootForm.angleTextBox.Tag).f;
                        matrix.Translate(0f - array2[0].X, 0f - array2[0].Y, MatrixOrder.Append);
                        matrix.Rotate(num - angle, MatrixOrder.Append);
                        matrix.Translate(array2[0].X, array2[0].Y, MatrixOrder.Append);
                        break;

                    case 3:
                        num = ((FloatParam)Form1.myRootForm.shearTextBox.Tag).f / 100f;
                        if (num < -2f)
                        {
                            num = -2f;
                        }
                        else if (num > 2f)
                        {
                            num = 2f;
                        }
                        sceneGroup.shear = num;
                        break;

                    case 10:
                        num = ((angle > 0f && angle <= 90f) ? 0f : ((angle > 90f && angle <= 180f) ? 90f : ((!(angle > -180f) || !(angle <= -90f)) ? (-90f) : 180f)));
                        matrix.Translate(0f - array2[0].X, 0f - array2[0].Y, MatrixOrder.Append);
                        matrix.Rotate(num - angle, MatrixOrder.Append);
                        matrix.Translate(array2[0].X, array2[0].Y, MatrixOrder.Append);
                        break;
                }
                sceneGroup.dirty = true;
                sceneGroup.transform.Reset();
                sceneGroup.transform.Multiply(sceneGroup.otm, MatrixOrder.Append);
                sceneGroup.transform.Multiply(matrix, MatrixOrder.Append);
                sceneGroup.updateNumBoxes();
            }
            SceneGroup[] array3 = new SceneGroup[selectedGroups.Length];
            for (int j = 0; j < selectedGroups.Length; j++)
            {
                array3[j] = (SceneGroup)selectedGroups[j].getCopy();
            }
            Form1.myRootForm.getCanvas().undoRedo.add(new UndoRedo.UndoTransformShapes(array3, selectedGroups));
        }

        public void transformGroup(SceneGroup baseGroup, DragRelativeTo dragRelativeTo, int handleTag, float hratio, float vratio, Matrix canvasToWorld, PointF mouseDownPnt, PointF mouseMovePnt)
        {
            if (baseGroup == null)
            {
                baseGroup = this;
            }
            float num = 0f;
            float num2 = 0f;
            float scaleX = 1f;
            float scaleY = 1f;
            float num3 = 0f;
            float num4 = 0f;
            PointF pointF = new PointF((bbox.Left + bbox.Right) / 2f, (bbox.Top + bbox.Bottom) / 2f);
            PointF pointF2 = new PointF((baseGroup.bbox.Left + baseGroup.bbox.Right) / 2f, (baseGroup.bbox.Top + baseGroup.bbox.Bottom) / 2f);
            Transformers transformers = new Transformers();
            Transformers transformers2 = new Transformers();
            Matrix matrix = new Matrix();
            float num5 = mouseMovePnt.X - mouseDownPnt.X;
            float num6 = mouseMovePnt.Y - mouseDownPnt.Y;
            PointF[] array = new PointF[2] { mouseDownPnt, mouseMovePnt };
            canvasToWorld.TransformPoints(array);
            float translateX = 0f;
            float translateY = 0f;
            switch (handleTag)
            {
                case 1:
                    transformers.calc(new PointF(bbox.Left, bbox.Top), array[1], array[0], otm);
                    transformers2.calc(new PointF(baseGroup.bbox.Left, baseGroup.bbox.Top), array[1], array[0], baseGroup.otm);
                    scaleX = transformers2.mlen / transformers2.hlen;
                    scaleY = transformers2.mlen / transformers2.hlen;
                    GetTranslateForRelative(dragRelativeTo, transformers, transformers2, ref translateX, ref translateY);
                    matrix.Translate(0f - translateX, 0f - translateY, MatrixOrder.Append);
                    matrix.Scale(scaleX, scaleY, MatrixOrder.Append);
                    matrix.Translate(translateX, translateY, MatrixOrder.Append);
                    break;

                case 2:
                    transformers.calc(new PointF((bbox.Left + bbox.Right) / 2f, bbox.Top), array[1], array[0], otm);
                    transformers2.calc(new PointF((baseGroup.bbox.Left + baseGroup.bbox.Right) / 2f, baseGroup.bbox.Top), array[1], array[0], baseGroup.otm);
                    scaleY = transformers2.mlen / transformers2.hlen;
                    GetTranslateForRelative(dragRelativeTo, transformers, transformers2, ref translateX, ref translateY);
                    matrix.Translate(0f - translateX, 0f - translateY, MatrixOrder.Append);
                    matrix.Scale(scaleX, scaleY, MatrixOrder.Append);
                    matrix.Translate(translateX, translateY, MatrixOrder.Append);
                    break;

                case 4:
                    transformers.calc(new PointF(bbox.Left, (bbox.Top + bbox.Bottom) / 2f), array[1], array[0], otm);
                    transformers2.calc(new PointF(baseGroup.bbox.Left, (baseGroup.bbox.Top + baseGroup.bbox.Bottom) / 2f), array[1], array[0], baseGroup.otm);
                    scaleX = transformers2.mlen / transformers2.hlen;
                    GetTranslateForRelative(dragRelativeTo, transformers, transformers2, ref translateX, ref translateY);
                    matrix.Translate(0f - translateX, 0f - translateY, MatrixOrder.Append);
                    matrix.Scale(scaleX, scaleY, MatrixOrder.Append);
                    matrix.Translate(translateX, translateY, MatrixOrder.Append);
                    break;

                case 3:
                    transformers.calc(new PointF(bbox.Left, bbox.Bottom), array[1], array[0], otm);
                    transformers2.calc(new PointF(baseGroup.bbox.Left, baseGroup.bbox.Bottom), array[1], array[0], baseGroup.otm);
                    num2 = ((!(Math.Abs(transformers2.hlen) > float.Epsilon)) ? 0f : (transformers2.hvec.X / transformers2.hlen * transformers2.nlen * transformers2.ndot));
                    shear = oldShear + num2;
                    break;

                case 0:
                case 5:
                    num3 = num5 / hratio;
                    num4 = num6 / vratio;
                    matrix.Translate(num3, num4, MatrixOrder.Append);
                    break;

                case 8:
                    num3 = num5 / hratio;
                    matrix.Translate(num3, 0f, MatrixOrder.Append);
                    break;

                case 6:
                    num4 = num6 / vratio;
                    matrix.Translate(0f, num4, MatrixOrder.Append);
                    break;

                case 7:
                    transformers.calc(new PointF(pointF.X, pointF.Y), array[1], array[0], otm);
                    transformers2.calc(new PointF(pointF2.X, pointF2.Y), array[1], array[0], baseGroup.otm);
                    num = (float)Math.Acos(transformers2.dot) * 180f / (float)Math.PI;
                    if (transformers2.hvec.X * transformers2.mvec.Y - transformers2.hvec.Y * transformers2.mvec.X < 0f)
                    {
                        num = 0f - num;
                    }
                    GetTranslateForRelative(dragRelativeTo, transformers, transformers2, ref translateX, ref translateY);
                    matrix.Translate(0f - translateX, 0f - translateY, MatrixOrder.Append);
                    matrix.Rotate(num, MatrixOrder.Append);
                    matrix.Translate(translateX, translateY, MatrixOrder.Append);
                    break;
            }
            dirty = true;
            transform.Reset();
            transform.Multiply(otm, MatrixOrder.Append);
            transform.Multiply(matrix, MatrixOrder.Append);
        }

        private void GetTranslateForRelative(DragRelativeTo dragRelativeTo, Transformers tf, Transformers tfBase, ref float translateX, ref float translateY)
        {
            switch (dragRelativeTo)
            {
                case DragRelativeTo.Self:
                    translateX = tf.anchor.X;
                    translateY = tf.anchor.Y;
                    break;

                case DragRelativeTo.PrimarySelection:
                    translateX = tfBase.anchor.X;
                    translateY = tfBase.anchor.Y;
                    break;
            }
        }
    }
}