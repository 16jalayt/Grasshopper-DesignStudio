using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Cricut_Design_Studio
{
	public class UndoRedo
	{
		public abstract class UndoAtom
		{
			public string name;

			public UndoAtom(string pname)
			{
				name = pname;
			}

			public abstract void undo(UndoRedo ur, Canvas canvas);

			public abstract void redo(UndoRedo ur, Canvas canvas);

			internal void PrintAction(bool undo, string a)
			{
			}

			internal void PrintUndoAction(string a)
			{
				PrintAction(undo: true, a);
			}

			internal void PrintRedoAction(string a)
			{
				PrintAction(undo: false, a);
			}
		}

		public abstract class UndoSceneGlyphAtom : UndoAtom
		{
			public SceneGroup[] sgs;

			public UndoSceneGlyphAtom(string pname, SceneGroup[] psgs)
				: base(pname)
			{
				sgs = psgs;
			}
		}

		public class UndoAddGlyph : UndoAtom
		{
			private int keyId;

			private float size;

			private float ox;

			private float oy;

			private float drawLocationX;

			private float drawLocationY;

			private FontLoading fontLoading;

			private SceneGroup group;

			public UndoAddGlyph(FontLoading pfontLoading, int pkeyId, float psize)
				: base("Add Image")
			{
				fontLoading = pfontLoading;
				keyId = pkeyId;
				size = psize;
			}

			public override void undo(UndoRedo ur, Canvas canvas)
			{
				PrintUndoAction("delete last from selected group, set cursor to " + (ox - drawLocationX) + ", " + (oy - drawLocationY) + " size " + size);
				if (Form1.myRootForm.getCanvas().selectedGroup != null)
				{
					Form1.myRootForm.getCanvas().setCursor(ox - drawLocationX, oy - drawLocationY, size, ignoreMaxX: true);
					Form1.myRootForm.getCanvas().selectedGroup.delLast();
				}
			}

			public override void redo(UndoRedo ur, Canvas canvas)
			{
				PrintRedoAction("add key " + keyId + " size " + size);
				fontLoading.addGlyphToCanvas(keyId, size);
				group = Form1.myRootForm.getCanvas().selectedGroup;
				if (group != null)
				{
					SceneGlyph sceneGlyph = (SceneGlyph)group.children[group.children.Count - 1];
					ox = sceneGlyph.ox;
					oy = sceneGlyph.oy;
					drawLocationX = canvas.drawR.X;
					drawLocationY = canvas.drawR.Y;
				}
			}
		}

		public class UndoSelectBase : UndoAtom
		{
			protected UndoSelectBase(string name)
				: base(name)
			{
			}

			protected void SelectGroup(Canvas canvas, string primarySelection, Hashtable selections, bool undo)
			{
				string text = "primary group " + primarySelection + " others ";
				SceneGroup sceneGroup = null;
				foreach (SceneGroup sceneGroup2 in canvas.sceneGroups)
				{
					string selectionUndoIdentity = sceneGroup2.SelectionUndoIdentity;
					if (selectionUndoIdentity == primarySelection)
					{
						sceneGroup = sceneGroup2;
					}
					if (selections.ContainsKey(selectionUndoIdentity))
					{
						sceneGroup2.selected = true;
					}
					else
					{
						sceneGroup2.selected = false;
					}
					text = text + " " + selectionUndoIdentity;
				}
				if (sceneGroup != null)
				{
					canvas.selectedGroup = sceneGroup;
					canvas.selectedGroup.selected = true;
				}
				PrintAction(undo, text);
			}

			public override void redo(UndoRedo ur, Canvas canvas)
			{
				throw new Exception("The method or operation is not implemented.");
			}

			public override void undo(UndoRedo ur, Canvas canvas)
			{
				throw new Exception("The method or operation is not implemented.");
			}
		}

		public class UndoSelectPoint : UndoSelectBase
		{
			private string previousPrimarySelection;

			private Hashtable previousSelections;

			private PointF pt;

			private bool clearPreviousSelections;

			public UndoSelectPoint(PointF p, bool pClearPreviousSelections)
				: base("Selection")
			{
				pt = p;
				clearPreviousSelections = pClearPreviousSelections;
			}

			public override void undo(UndoRedo ur, Canvas canvas)
			{
				SelectGroup(canvas, previousPrimarySelection, previousSelections, undo: true);
			}

			public override void redo(UndoRedo ur, Canvas canvas)
			{
				PrintRedoAction(" select " + pt.X + ", " + pt.Y + " " + clearPreviousSelections);
				if (canvas.selectedGroup != null)
				{
					previousPrimarySelection = canvas.selectedGroup.SelectionUndoIdentity;
				}
				else
				{
					previousPrimarySelection = null;
				}
				previousSelections = new Hashtable();
				SceneGroup[] selectedGroups = canvas.GetSelectedGroups();
				SceneGroup[] array = selectedGroups;
				foreach (SceneGroup sceneGroup in array)
				{
					previousSelections[sceneGroup.SelectionUndoIdentity] = true;
				}
				canvas.select(pt, clearPreviousSelections);
			}
		}

		public class UndoSelect : UndoSelectBase
		{
			private string previousPrimarySelection;

			private Hashtable previousSelections;

			private string newPrimarySelection;

			private Hashtable newSelections;

			public UndoSelect(Canvas canvas)
				: base("Selections")
			{
				if (canvas.selectedGroup != null)
				{
					previousPrimarySelection = canvas.selectedGroup.SelectionUndoIdentity;
				}
				else
				{
					previousPrimarySelection = null;
				}
				previousSelections = new Hashtable();
				SceneGroup[] selectedGroups = canvas.GetSelectedGroups();
				SceneGroup[] array = selectedGroups;
				foreach (SceneGroup sceneGroup in array)
				{
					previousSelections[sceneGroup.SelectionUndoIdentity] = true;
				}
			}

			public void SaveNewSelections(Canvas canvas)
			{
				if (canvas.selectedGroup != null)
				{
					newPrimarySelection = canvas.selectedGroup.SelectionUndoIdentity;
				}
				else
				{
					newPrimarySelection = null;
				}
				newSelections = new Hashtable();
				SceneGroup[] selectedGroups = canvas.GetSelectedGroups();
				SceneGroup[] array = selectedGroups;
				foreach (SceneGroup sceneGroup in array)
				{
					newSelections[sceneGroup.SelectionUndoIdentity] = true;
				}
			}

			public override void undo(UndoRedo ur, Canvas canvas)
			{
				SelectGroup(canvas, previousPrimarySelection, previousSelections, undo: true);
			}

			public override void redo(UndoRedo ur, Canvas canvas)
			{
				SelectGroup(canvas, previousPrimarySelection, previousSelections, undo: false);
			}
		}

		public class UndoDelGlyph : UndoAtom
		{
			private int keyId;

			private float size;

			private float ox;

			private float oy;

			private float drawLocationX;

			private float drawLocationY;

			private FontLoading fontLoading;

			private SceneGroup group;

			public UndoDelGlyph(SceneGroup pgroup, FontLoading pfontLoading, int pkeyId, float psize, float pox, float poy, float pdrawLocationX, float pdrawLocationY)
				: base("Delete Image")
			{
				fontLoading = pfontLoading;
				group = pgroup;
				ox = pox;
				oy = poy;
				drawLocationX = pdrawLocationX;
				drawLocationY = pdrawLocationY;
				keyId = pkeyId;
				size = psize;
			}

			public override void undo(UndoRedo ur, Canvas canvas)
			{
				PrintUndoAction("adding " + keyId + " size " + size);
				fontLoading.addGlyphToCanvas(keyId, size);
			}

			public override void redo(UndoRedo ur, Canvas canvas)
			{
				PrintRedoAction("cursor to " + (ox - drawLocationX) + ", " + (oy - drawLocationY) + " size " + size);
				Form1.myRootForm.getCanvas().setCursor(ox - drawLocationX, oy - drawLocationY, size, ignoreMaxX: true);
				group.delLast();
			}
		}

		public class UndoDeleteShapes : UndoSceneGlyphAtom
		{
			private PCImage[] images;

			public UndoDeleteShapes(SceneGroup[] psg, PCImage[] pimages)
				: base("Delete Shapes", psg)
			{
				images = pimages;
			}

			public override void undo(UndoRedo ur, Canvas canvas)
			{
				string text = "";
				if (canvas.selectedGroup != null)
				{
					canvas.selectedGroup.selected = false;
				}
				SceneGlyph.selectedGlyph = null;
				SceneGroup[] array = sgs;
				foreach (SceneGroup sceneGroup in array)
				{
					if (text != "")
					{
						text += ", ";
					}
					text += sceneGroup.ToString();
					canvas.sceneGroups.Add(sceneGroup);
					canvas.Select(sceneGroup);
				}
				PrintUndoAction("added scene groups " + text);
				Form1.myRootForm.refreshMattePicBox();
			}

			public override void redo(UndoRedo ur, Canvas canvas)
			{
				string text = "";
				SceneGroup[] array = sgs;
				foreach (SceneGroup sceneGroup in array)
				{
					if (text != "")
					{
						text += ", ";
					}
					text += sceneGroup.ToString();
					canvas.removeGroup(sceneGroup);
				}
				PrintRedoAction("deleted groups " + text);
			}
		}

		public class UndoTransformShapes : UndoSceneGlyphAtom
		{
			private SceneGroup[] parents;

			public UndoTransformShapes(SceneGroup[] psg, SceneGroup[] pparents)
				: base("Transform Shapes", psg)
			{
				parents = pparents;
			}

			private void swapSceneGroupTransform(SceneGroup sg1, SceneGroup sg2)
			{
				float oldShear = sg1.oldShear;
				sg1.oldShear = sg2.oldShear;
				sg2.oldShear = oldShear;
				oldShear = sg1.shear;
				sg1.shear = sg2.shear;
				sg2.shear = oldShear;
				Matrix otm = sg1.otm;
				sg1.otm = sg2.otm;
				sg2.otm = otm;
				otm = sg1.transform;
				sg1.transform = sg2.transform;
				sg2.transform = otm;
				RectangleF bbox = sg1.bbox;
				sg1.bbox = sg2.bbox;
				sg2.bbox = bbox;
				bool renderType = sg2.renderType;
				sg1.renderType = sg2.renderType;
				sg2.renderType = renderType;
			}

			private void undoRedoTransform(Canvas canvas, bool undo)
			{
				string text = "";
				canvas.deselectAll();
				for (int i = 0; i < sgs.Length; i++)
				{
					SceneGroup sceneGroup = sgs[i];
					SceneGroup obj = parents[i];
					foreach (SceneGroup sceneGroup2 in canvas.sceneGroups)
					{
						if (sceneGroup2.Equals(obj))
						{
							if (text != "")
							{
								text += ", ";
							}
							text += sceneGroup.ToString();
							canvas.Select(sceneGroup2);
							swapSceneGroupTransform(sceneGroup2, sceneGroup);
							sceneGroup2.calcBbox(null);
							break;
						}
					}
				}
				PrintAction(undo, "swapped transforms for " + text);
			}

			public override void undo(UndoRedo ur, Canvas canvas)
			{
				undoRedoTransform(canvas, undo: true);
			}

			public override void redo(UndoRedo ur, Canvas canvas)
			{
				undoRedoTransform(canvas, undo: false);
			}
		}

		private ArrayList undoAtoms = new ArrayList();

		private ArrayList redoAtoms = new ArrayList();

		public Canvas canvas;

		public UndoRedo(Canvas c)
		{
			canvas = c;
		}

		public void addAndDo(UndoAtom atom)
		{
			add(atom);
			atom.redo(this, canvas);
		}

		public void add(UndoAtom atom)
		{
			undoAtoms.Add(atom);
			redoAtoms.Clear();
			Form1.myRootForm.undoToolStripMenuItem.Text = "Undo " + atom.name;
			Form1.myRootForm.undoToolStripMenuItem.Enabled = true;
		}

		public void undo()
		{
			if (undoAtoms.Count >= 1)
			{
				UndoAtom undoAtom = (UndoAtom)undoAtoms[undoAtoms.Count - 1];
				undoAtom.undo(this, canvas);
				redoAtoms.Add(undoAtom);
				undoAtoms.Remove(undoAtom);
				Form1.myRootForm.refreshMattePicBox();
				refreshMenu();
			}
		}

		public void redo()
		{
			if (redoAtoms.Count >= 1)
			{
				UndoAtom undoAtom = (UndoAtom)redoAtoms[redoAtoms.Count - 1];
				undoAtom.redo(this, canvas);
				undoAtoms.Add(undoAtom);
				redoAtoms.Remove(undoAtom);
				Form1.myRootForm.refreshMattePicBox();
				refreshMenu();
			}
		}

		public void clear()
		{
			undoAtoms.Clear();
			redoAtoms.Clear();
			refreshMenu();
		}

		private void refreshMenu()
		{
			if (undoAtoms.Count > 0)
			{
				UndoAtom undoAtom = (UndoAtom)undoAtoms[undoAtoms.Count - 1];
				Form1.myRootForm.undoToolStripMenuItem.Text = "Undo " + undoAtom.name;
				Form1.myRootForm.undoToolStripMenuItem.Enabled = true;
			}
			else
			{
				Form1.myRootForm.undoToolStripMenuItem.Text = "Undo";
				Form1.myRootForm.undoToolStripMenuItem.Enabled = false;
			}
			if (redoAtoms.Count > 0)
			{
				UndoAtom undoAtom2 = (UndoAtom)redoAtoms[redoAtoms.Count - 1];
				Form1.myRootForm.redoToolStripMenuItem.Text = "Redo " + undoAtom2.name;
				Form1.myRootForm.redoToolStripMenuItem.Enabled = true;
			}
			else
			{
				Form1.myRootForm.redoToolStripMenuItem.Text = "Redo";
				Form1.myRootForm.redoToolStripMenuItem.Enabled = false;
			}
		}
	}
}
