//
// TextEditor.cs
//
// Author:
//       Mike Krüger <mkrueger@xamarin.com>
//
// Copyright (c) 2012 Xamarin Inc. (http://xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.



using System;
using System.Collections.Generic;
using System.Linq;
using Gdk;
using Gtk;
using Mono.TextEditor.Highlighting;
using Mono.TextEditor.Theatrics;


namespace Mono.TextEditor
{
	[System.ComponentModel.Category("Mono.TextEditor")]
	[System.ComponentModel.ToolboxItem(true)]
	public class TextEditor : Container
	{
		readonly TextArea textEditorWidget;

		internal TextArea TextArea {
			get {
				return textEditorWidget;
			}
		}

		public override ContainerChild this [Widget w] {
			get {
				return containerChildren.FirstOrDefault (info => info.Child == w || (info.Child is AnimatedWidget && ((AnimatedWidget)info.Child).Widget == w));
			}
		}

		public TextEditor () : this(new TextDocument ())
		{
		}

		public TextEditor (TextDocument doc)
			: this (doc, null)
		{
		}
		
		public TextEditor (TextDocument doc, ITextEditorOptions options)
			: this (doc, options, new SimpleEditMode ())
		{
		}

		public TextEditor (TextDocument doc, ITextEditorOptions options, EditMode initialMode)
		{
			GtkWorkarounds.FixContainerLeak (this);
			WidgetFlags |= Gtk.WidgetFlags.NoWindow;
			this.textEditorWidget = new TextArea (this, doc, options, initialMode);
			this.textEditorWidget.Initialize ();
			this.textEditorWidget.EditorOptionsChanged += (sender, e) => OptionsChanged (sender, e);
		
			AddTopLevelWidget (textEditorWidget, 0, 0);
			stage.ActorStep += OnActorStep;
			ShowAll ();
			
			// bug on mac: search widget gets overdrawn in the scroll event.
			if (Platform.IsMac) {
				textEditorWidget.VScroll += delegate {
					for (int i = 1; i < containerChildren.Count; i++) {
						containerChildren[i].Child.QueueDraw ();
					}
				};
				textEditorWidget.HScroll += delegate {
					for (int i = 1; i < containerChildren.Count; i++) {
						containerChildren[i].Child.QueueDraw ();
					}
				};
			}
		}
		
		public class EditorContainerChild : Container.ContainerChild
		{
			public int X { get; set; }
			public int Y { get; set; }
			public bool FixedPosition { get; set; }
			public EditorContainerChild (Container parent, Widget child) : base (parent, child)
			{
			}
		}
		
		public override GLib.GType ChildType ()
		{
			return Gtk.Widget.GType;
		}
		
		List<EditorContainerChild> containerChildren = new List<EditorContainerChild> ();
		
		public void AddTopLevelWidget (Gtk.Widget widget, int x, int y)
		{
			widget.Parent = this;
			EditorContainerChild info = new EditorContainerChild (this, widget);
			info.X = x;
			info.Y = y;
			containerChildren.Add (info);
		}
		
		public void MoveTopLevelWidget (Gtk.Widget widget, int x, int y)
		{
			foreach (EditorContainerChild info in containerChildren.ToArray ()) {
				if (info.Child == widget || (info.Child is AnimatedWidget && ((AnimatedWidget)info.Child).Widget == widget)) {
					if (info.X == x && info.Y == y)
						break;
					info.X = x;
					info.Y = y;
					if (widget.Visible)
						ResizeChild (Allocation, info);
					break;
				}
			}
		}
		
		public void MoveToTop (Gtk.Widget widget)
		{
			EditorContainerChild editorContainerChild = containerChildren.FirstOrDefault (c => c.Child == widget);
			if (editorContainerChild == null)
				throw new Exception ("child " + widget + " not found.");
			List<EditorContainerChild> newChilds = new List<EditorContainerChild> (containerChildren.Where (child => child != editorContainerChild));
			newChilds.Add (editorContainerChild);
			this.containerChildren = newChilds;
			widget.GdkWindow.Raise ();
		}
		
		protected override void OnAdded (Widget widget)
		{
			AddTopLevelWidget (widget, 0, 0);
		}
		
		protected override void OnRemoved (Widget widget)
		{
			foreach (EditorContainerChild info in containerChildren.ToArray ()) {
				if (info.Child == widget) {
					widget.Unparent ();
					containerChildren.Remove (info);
					break;
				}
			}
		}
		
		protected override void ForAll (bool include_internals, Gtk.Callback callback)
		{
			containerChildren.ForEach (child => callback (child.Child));
		}
		/*
		protected override void OnMapped ()
		{
			WidgetFlags |= WidgetFlags.Mapped;
			// Note: SourceEditorWidget.ShowAutoSaveWarning() might have set TextEditor.Visible to false,
			// in which case we want to not map it (would cause a gtk+ critical error).
			containerChildren.ForEach (child => { if (child.Child.Visible) child.Child.Map (); });
			GdkWindow.Show ();
		}
		
		protected override void OnUnmapped ()
		{
			WidgetFlags &= ~WidgetFlags.Mapped;
			
			// We hide the window first so that the user doesn't see widgets disappearing one by one.
			GdkWindow.Hide ();
			
			containerChildren.ForEach (child => child.Child.Unmap ());
		}

		protected override void OnRealized ()
		{
			WidgetFlags |= WidgetFlags.Realized;
			WindowAttr attributes = new WindowAttr () {
				WindowType = Gdk.WindowType.Child,
				X = Allocation.X,
				Y = Allocation.Y,
				Width = Allocation.Width,
				Height = Allocation.Height,
				Wclass = WindowClass.InputOutput,
				Visual = this.Visual,
				Colormap = this.Colormap,
				EventMask = (int)(this.Events | Gdk.EventMask.ExposureMask),
				Mask = this.Events | Gdk.EventMask.ExposureMask,
			};
			
			WindowAttributesType mask = WindowAttributesType.X | WindowAttributesType.Y | WindowAttributesType.Colormap | WindowAttributesType.Visual;
			GdkWindow = new Gdk.Window (ParentWindow, attributes, mask);
			GdkWindow.UserData = Raw;
			Style = Style.Attach (GdkWindow);
		}
		
		protected override void OnUnrealized ()
		{
			WidgetFlags &= ~WidgetFlags.Realized;
			GdkWindow.Dispose ();
			base.OnUnrealized ();
		}*/
		
		protected override void OnSizeAllocated (Rectangle allocation)
		{
			base.OnSizeAllocated (allocation);
			if (this.GdkWindow != null)
				this.GdkWindow.MoveResize (allocation);
			allocation = new Rectangle (0, 0, allocation.Width, allocation.Height);
			if (textEditorWidget.Allocation != allocation)
				textEditorWidget.SizeAllocate (allocation);
			SetChildrenPositions (allocation);
		}
		
		void ResizeChild (Rectangle allocation, EditorContainerChild child)
		{
			Requisition req = child.Child.SizeRequest ();
			var childRectangle = new Gdk.Rectangle (child.X, child.Y, req.Width, req.Height);
			if (!child.FixedPosition) {
				double zoom = textEditorWidget.Options.Zoom;
				childRectangle.X = (int)(child.X * zoom - textEditorWidget.HAdjustment.Value);
				childRectangle.Y = (int)(child.Y * zoom - textEditorWidget.VAdjustment.Value);
			}
			//			childRectangle.X += allocation.X;
			//			childRectangle.Y += allocation.Y;
			child.Child.SizeAllocate (childRectangle);
		}
		
		void SetChildrenPositions (Rectangle allocation)
		{
			foreach (EditorContainerChild child in containerChildren.ToArray ()) {
				if (child.Child == textEditorWidget)
					continue;
				ResizeChild (allocation, child);
			}
		}
		
		#region Animated Widgets
		Stage<AnimatedWidget> stage = new Stage<AnimatedWidget> ();
		
		bool OnActorStep (Actor<AnimatedWidget> actor)
		{
			switch (actor.Target.AnimationState) {
			case AnimationState.Coming:
				actor.Target.QueueDraw ();
				actor.Target.Percent = actor.Percent;
				if (actor.Expired) {
					actor.Target.AnimationState = AnimationState.Idle;
					return false;
				}
				break;
			case AnimationState.IntendingToGo:
				actor.Target.AnimationState = AnimationState.Going;
				actor.Target.Bias = actor.Percent;
				actor.Reset ((uint)(actor.Target.Duration * actor.Percent));
				break;
			case AnimationState.Going:
				if (actor.Expired) {
					this.Remove (actor.Target);
					return false;
				}
				actor.Target.Percent = 1.0 - actor.Percent;
				break;
			}
			return true;
		}
		
		void OnWidgetDestroyed (object sender, EventArgs args)
		{
			RemoveCore ((AnimatedWidget)sender);
		}
		
		void RemoveCore (AnimatedWidget widget)
		{
			RemoveCore (widget, widget.Duration, 0, 0, false, false);
		}
		
		void RemoveCore (AnimatedWidget widget, uint duration, Easing easing, Blocking blocking, bool use_easing, bool use_blocking)
		{
			if (duration > 0)
				widget.Duration = duration;
			
			if (use_easing)
				widget.Easing = easing;
			
			if (use_blocking)
				widget.Blocking = blocking;
			
			if (widget.AnimationState == AnimationState.Coming) {
				widget.AnimationState = AnimationState.IntendingToGo;
			} else {
				if (widget.Easing == Easing.QuadraticIn) {
					widget.Easing = Easing.QuadraticOut;
				} else if (widget.Easing == Easing.QuadraticOut) {
					widget.Easing = Easing.QuadraticIn;
				} else if (widget.Easing == Easing.ExponentialIn) {
					widget.Easing = Easing.ExponentialOut;
				} else if (widget.Easing == Easing.ExponentialOut) {
					widget.Easing = Easing.ExponentialIn;
				}
				widget.AnimationState = AnimationState.Going;
				stage.Add (widget, widget.Duration);
			}
		}
		
		public void AddAnimatedWidget (Widget widget, uint duration, Easing easing, Blocking blocking, int x, int y)
		{
			AnimatedWidget animated_widget = new AnimatedWidget (widget, duration, easing, blocking, false);
			animated_widget.Parent = this;
			animated_widget.WidgetDestroyed += OnWidgetDestroyed;
			stage.Add (animated_widget, duration);
			animated_widget.StartPadding = 0;
			animated_widget.EndPadding = widget.Allocation.Height;
			//			animated_widget.Node = animated_widget;
			
			EditorContainerChild info = new EditorContainerChild (this, animated_widget);
			info.X = x;
			info.Y = y;
			info.FixedPosition = true;
			containerChildren.Add (info);
		}
		#endregion

		Adjustment editorHAdjustement;
		Adjustment editorVAdjustement;

		protected sealed override void OnSetScrollAdjustments (Adjustment hAdjustement, Adjustment vAdjustement)
		{
			if (editorHAdjustement != null)
				editorHAdjustement.ValueChanged -= HandleHAdjustementValueChanged;
			if (editorVAdjustement != null)
				editorVAdjustement.ValueChanged -= HandleHAdjustementValueChanged;
			
			if (hAdjustement != null)
				hAdjustement.ValueChanged += HandleHAdjustementValueChanged;
			if (vAdjustement != null)
				vAdjustement.ValueChanged += HandleHAdjustementValueChanged;
			
			editorHAdjustement = hAdjustement;
			editorVAdjustement = vAdjustement;
			textEditorWidget.SetScrollAdjustments (hAdjustement, vAdjustement);
			OnScrollAdjustmentsSet ();
		}

		protected virtual void OnScrollAdjustmentsSet ()
		{
		}
		
		void HandleHAdjustementValueChanged (object sender, EventArgs e)
		{
			var alloc = this.Allocation;
			alloc.X = alloc.Y = 0;
			SetChildrenPositions (alloc);
		}
		
		protected override void OnDestroyed ()
		{
			if (editorHAdjustement != null)
				editorHAdjustement.ValueChanged -= HandleHAdjustementValueChanged;
			if (editorVAdjustement != null)
				editorVAdjustement.ValueChanged -= HandleHAdjustementValueChanged;
			base.OnDestroyed ();
		}

		#region TextArea delegation
		public TextDocument Document {
			get {
				return textEditorWidget.Document;
			}
		}
		
		public bool IsDisposed {
			get {
				return textEditorWidget.IsDisposed;
			}
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Mono.TextEditor.TextEditor"/> converts tabs to spaces.
		/// It is possible to overwrite the default options value for certain languages (like F#).
		/// </summary>
		/// <value>
		/// <c>true</c> if tabs to spaces should be converted; otherwise, <c>false</c>.
		/// </value>
		public bool TabsToSpaces {
			get {
				return textEditorWidget.TabsToSpaces;
			}
			set {
				textEditorWidget.TabsToSpaces = value;
			}
		}

		public Mono.TextEditor.Caret Caret {
			get {
				return textEditorWidget.Caret;
			}
		}

		protected internal IMMulticontext IMContext {
			get { return textEditorWidget.IMContext; }
		}

		public string IMModule {
			get {
				return textEditorWidget.IMModule;
			}
			set {
				textEditorWidget.IMModule = value;
			}
		}

		public ITextEditorOptions Options {
			get {
				return textEditorWidget.Options;
			}
			set {
				textEditorWidget.Options = value;
			}
		}

		public string FileName {
			get {
				return textEditorWidget.FileName;
			}
		}

		public string MimeType {
			get {
				return textEditorWidget.MimeType;
			}
		}

		public double LineHeight {
			get {
				return textEditorWidget.LineHeight;
			}
			internal set {
				textEditorWidget.LineHeight = value;
			}
		}

		public TextViewMargin TextViewMargin {
			get {
				return textEditorWidget.TextViewMargin;
			}
		}

		public Margin IconMargin {
			get { return textEditorWidget.IconMargin; }
		}

		public DocumentLocation LogicalToVisualLocation (DocumentLocation location)
		{
			return textEditorWidget.LogicalToVisualLocation (location);
		}
		
		public DocumentLocation LogicalToVisualLocation (int line, int column)
		{
			return textEditorWidget.LogicalToVisualLocation (line, column);
		}
		
		public void CenterToCaret ()
		{
			textEditorWidget.CenterToCaret ();
		}
		
		public void CenterTo (int offset)
		{
			textEditorWidget.CenterTo (offset);
		}
		
		public void CenterTo (int line, int column)
		{
			textEditorWidget.CenterTo (line, column);
		}
		
		public void CenterTo (DocumentLocation p)
		{
			textEditorWidget.CenterTo (p);
		}

		internal void SmoothScrollTo (double value)
		{
			textEditorWidget.SmoothScrollTo (value);
		}

		public void ScrollTo (int offset)
		{
			textEditorWidget.ScrollTo (offset);
		}
		
		public void ScrollTo (int line, int column)
		{
			textEditorWidget.ScrollTo (line, column);
		}

		public void ScrollTo (DocumentLocation p)
		{
			textEditorWidget.ScrollTo (p);
		}

		public void ScrollToCaret ()
		{
			textEditorWidget.ScrollToCaret ();
		}

		public void TryToResetHorizontalScrollPosition ()
		{
			textEditorWidget.TryToResetHorizontalScrollPosition ();
		}

		public int GetWidth (string text)
		{
			return textEditorWidget.GetWidth (text);
		}

		internal void HideMouseCursor ()
		{
			textEditorWidget.HideMouseCursor ();
		}

		public void ClearTooltipProviders ()
		{
			textEditorWidget.ClearTooltipProviders ();
		}
		
		public void AddTooltipProvider (TooltipProvider provider)
		{
			textEditorWidget.AddTooltipProvider (provider);
		}
		
		public void RemoveTooltipProvider (TooltipProvider provider)
		{
			textEditorWidget.RemoveTooltipProvider (provider);
		}

		internal void RedrawMargin (Margin margin)
		{
			textEditorWidget.RedrawMargin (margin);
		}
		
		public void RedrawMarginLine (Margin margin, int logicalLine)
		{
			textEditorWidget.RedrawMarginLine (margin, logicalLine);
		}
		internal void RedrawPosition (int logicalLine, int logicalColumn)
		{
			textEditorWidget.RedrawPosition (logicalLine, logicalColumn);
		}
#endregion

		#region TextEditorData delegation
		public string EolMarker {
			get {
				return textEditorWidget.EolMarker;
			}
		}
		
		public Mono.TextEditor.Highlighting.ColorScheme ColorStyle {
			get {
				return textEditorWidget.ColorStyle;
			}
		}
		
		public EditMode CurrentMode {
			get {
				return textEditorWidget.CurrentMode;
			}
			set {
				textEditorWidget.CurrentMode = value;
			}
		}
		
		public bool IsSomethingSelected {
			get {
				return textEditorWidget.IsSomethingSelected;
			}
		}
		
		public Selection MainSelection {
			get {
				return textEditorWidget.MainSelection;
			}
			set {
				textEditorWidget.MainSelection = value;
			}
		}
		
		public SelectionMode SelectionMode {
			get {
				return textEditorWidget.SelectionMode;
			}
			set {
				textEditorWidget.SelectionMode = value;
			}
		}
		
		public TextSegment SelectionRange {
			get {
				return textEditorWidget.SelectionRange;
			}
			set {
				textEditorWidget.SelectionRange = value;
			}
		}
		
		public string SelectedText {
			get {
				return textEditorWidget.SelectedText;
			}
			set {
				textEditorWidget.SelectedText = value;
			}
		}
		
		public int SelectionAnchor {
			get {
				return textEditorWidget.SelectionAnchor;
			}
			set {
				textEditorWidget.SelectionAnchor = value;
			}
		}
		
		public IEnumerable<DocumentLine> SelectedLines {
			get {
				return textEditorWidget.SelectedLines;
			}
		}
		
		public Adjustment HAdjustment {
			get {
				return textEditorWidget.HAdjustment;
			}
		}
		
		public Adjustment VAdjustment {
			get {
				return textEditorWidget.VAdjustment;
			}
		}
		
		public int Insert (int offset, string value)
		{
			return textEditorWidget.Insert (offset, value);
		}
		
		public void Remove (DocumentRegion region)
		{
			textEditorWidget.Remove (region);
		}
		
		public void Remove (TextSegment removeSegment)
		{
			textEditorWidget.Remove (removeSegment);
		}
		
		public void Remove (int offset, int count)
		{
			textEditorWidget.Remove (offset, count);
		}
		
		public int Replace (int offset, int count, string value)
		{
			return textEditorWidget.Replace (offset, count, value);
		}
		
		public void ClearSelection ()
		{
			textEditorWidget.ClearSelection ();
		}
		
		public void DeleteSelectedText ()
		{
			textEditorWidget.DeleteSelectedText ();
		}
		
		public void DeleteSelectedText (bool clearSelection)
		{
			textEditorWidget.DeleteSelectedText (clearSelection);
		}
		
		public void RunEditAction (Action<TextEditorData> action)
		{
			action (GetTextEditorData ());
		}
		
		public void SetSelection (int anchorOffset, int leadOffset)
		{
			textEditorWidget.SetSelection (anchorOffset, leadOffset);
		}
		
		public void SetSelection (DocumentLocation anchor, DocumentLocation lead)
		{
			textEditorWidget.SetSelection (anchor, lead);
		}
		
		public void SetSelection (int anchorLine, int anchorColumn, int leadLine, int leadColumn)
		{
			textEditorWidget.SetSelection (anchorLine, anchorColumn, leadLine, leadColumn);
		}
		
		public void ExtendSelectionTo (DocumentLocation location)
		{
			textEditorWidget.ExtendSelectionTo (location);
		}
		public void ExtendSelectionTo (int offset)
		{
			textEditorWidget.ExtendSelectionTo (offset);
		}
		public void SetSelectLines (int from, int to)
		{
			textEditorWidget.SetSelectLines (from, to);
		}
		
		public void InsertAtCaret (string text)
		{
			textEditorWidget.InsertAtCaret (text);
		}
		
		public bool CanEdit (int line)
		{
			return textEditorWidget.CanEdit (line);
		}
		
		public string GetLineText (int line)
		{
			return textEditorWidget.GetLineText (line);
		}
		
		public string GetLineText (int line, bool includeDelimiter)
		{
			return textEditorWidget.GetLineText (line, includeDelimiter);
		}
		
		/// <summary>
		/// Use with care.
		/// </summary>
		/// <returns>
		/// A <see cref="TextEditorData"/>
		/// </returns>
		public TextEditorData GetTextEditorData ()
		{
			return textEditorWidget.GetTextEditorData ();
		}

		/// <remarks>
		/// The Key may be null if it has been handled by the IMContext. In such cases, the char is the value.
		/// </remarks>
		protected internal virtual bool OnIMProcessedKeyPressEvent (Gdk.Key key, uint ch, Gdk.ModifierType state)
		{
			SimulateKeyPress (key, ch, state);
			return true;
		}

		public void SimulateKeyPress (Gdk.Key key, uint unicodeChar, ModifierType modifier)
		{
			textEditorWidget.SimulateKeyPress (key, unicodeChar, modifier);
		}

		
		public void RunAction (Action<TextEditorData> action)
		{
			try {
				action (GetTextEditorData ());
			} catch (Exception e) {
				Console.WriteLine ("Error while executing " + action + " :" + e);
			}
		}

		public void HideTooltip ()
		{
			textEditorWidget.HideTooltip ();
		}
		public Action<Gdk.EventButton> DoPopupMenu {
			get {
				return textEditorWidget.DoPopupMenu;
			}
			set {
				textEditorWidget.DoPopupMenu = value;
			} 
		}

		public MenuItem CreateInputMethodMenuItem (string label)
		{
			return textEditorWidget.CreateInputMethodMenuItem (label);
		}

		public event EventHandler SelectionChanged {
			add { textEditorWidget.SelectionChanged += value; }
			remove { textEditorWidget.SelectionChanged -= value; }
		}

		public void CaretToDragCaretPosition ()
		{
			textEditorWidget.CaretToDragCaretPosition ();
		}

		public event EventHandler<PaintEventArgs> Painted {
			add { textEditorWidget.Painted += value; }
			remove { textEditorWidget.Painted -= value; }
		}

		public event EventHandler<LinkEventArgs> LinkRequest {
			add { textEditorWidget.LinkRequest += value; }
			remove { textEditorWidget.LinkRequest -= value; }
		}
		#endregion
		
		#region Document delegation

		public event EventHandler EditorOptionsChanged {
			add { textEditorWidget.EditorOptionsChanged += value; }
			remove { textEditorWidget.EditorOptionsChanged -= value; }
		}

		protected virtual void OptionsChanged (object sender, EventArgs args)
		{
		}

		public int Length {
			get {
				return Document.TextLength;
			}
		}
		
		public string Text {
			get {
				return Document.Text;
			}
			set {
				Document.Text = value;
			}
		}
		
		public string GetTextBetween (int startOffset, int endOffset)
		{
			return Document.GetTextBetween (startOffset, endOffset);
		}
		
		public string GetTextBetween (DocumentLocation start, DocumentLocation end)
		{
			return Document.GetTextBetween (start, end);
		}
		
		public string GetTextBetween (int startLine, int startColumn, int endLine, int endColumn)
		{
			return Document.GetTextBetween (startLine, startColumn, endLine, endColumn);
		}
		
		public string GetTextAt (int offset, int count)
		{
			return Document.GetTextAt (offset, count);
		}
		
		public string GetTextAt (TextSegment segment)
		{
			return Document.GetTextAt (segment);
		}
		
		public string GetTextAt (DocumentRegion region)
		{
			return Document.GetTextAt (region);
		}
		
		public char GetCharAt (int offset)
		{
			return Document.GetCharAt (offset);
		}
		
		public IEnumerable<DocumentLine> Lines {
			get {
				return Document.Lines;
			}
		}
		
		public int LineCount {
			get {
				return Document.LineCount;
			}
		}
		
		public int LocationToOffset (int line, int column)
		{
			return Document.LocationToOffset (line, column);
		}
		
		public int LocationToOffset (DocumentLocation location)
		{
			return Document.LocationToOffset (location);
		}
		
		public DocumentLocation OffsetToLocation (int offset)
		{
			return Document.OffsetToLocation (offset);
		}
		
		public string GetLineIndent (int lineNumber)
		{
			return Document.GetLineIndent (lineNumber);
		}
		
		public string GetLineIndent (DocumentLine segment)
		{
			return Document.GetLineIndent (segment);
		}
		
		public DocumentLine GetLine (int lineNumber)
		{
			return Document.GetLine (lineNumber);
		}
		
		public DocumentLine GetLineByOffset (int offset)
		{
			return Document.GetLineByOffset (offset);
		}
		
		public int OffsetToLineNumber (int offset)
		{
			return Document.OffsetToLineNumber (offset);
		}
		
		public IDisposable OpenUndoGroup()
		{
			return Document.OpenUndoGroup ();
		}
#endregion
		
		#region Search & Replace
		public string SearchPattern {
			get {
				return textEditorWidget.SearchPattern;
			}
			set {
				textEditorWidget.SearchPattern = value;
			}
		}
		
		public ISearchEngine SearchEngine {
			get {
				return textEditorWidget.SearchEngine;
			}
			set {
				textEditorWidget.SearchEngine = value;
			}
		}

		public event EventHandler HighlightSearchPatternChanged {
			add { textEditorWidget.HighlightSearchPatternChanged += value; }
			remove { textEditorWidget.HighlightSearchPatternChanged -= value; }
		}

		public bool HighlightSearchPattern {
			get {
				return textEditorWidget.HighlightSearchPattern;
			}
			set {
				textEditorWidget.HighlightSearchPattern = value;
			}
		}
		
		public bool IsCaseSensitive {
			get {
				return textEditorWidget.IsCaseSensitive;
			}
			set {
				textEditorWidget.IsCaseSensitive = value;
			}
		}
		
		public bool IsWholeWordOnly {
			get {
				return textEditorWidget.IsWholeWordOnly;
			}
			
			set {
				textEditorWidget.IsWholeWordOnly = value;
			}
		}
		
		public TextSegment SearchRegion {
			get {
				return textEditorWidget.SearchRegion;
			}
			
			set {
				textEditorWidget.SearchRegion = value;
			}
		}
		
		public SearchResult SearchForward (int fromOffset)
		{
			return textEditorWidget.SearchForward (fromOffset);
		}
		
		public SearchResult SearchBackward (int fromOffset)
		{
			return textEditorWidget.SearchBackward (fromOffset);
		}
		
		/// <summary>
		/// Initiate a pulse at the specified document location
		/// </summary>
		/// <param name="pulseLocation">
		/// A <see cref="DocumentLocation"/>
		/// </param>
		public void PulseCharacter (DocumentLocation pulseStart)
		{
			textEditorWidget.PulseCharacter (pulseStart);
		}
		
		
		public SearchResult FindNext (bool setSelection)
		{
			return textEditorWidget.FindNext (setSelection);
		}
		
		public void StartCaretPulseAnimation ()
		{
			textEditorWidget.StartCaretPulseAnimation ();
		}

		public void StopSearchResultAnimation ()
		{
			textEditorWidget.StopSearchResultAnimation ();
		}
		
		public void AnimateSearchResult (SearchResult result)
		{
			textEditorWidget.AnimateSearchResult (result);
		}

		public SearchResult FindPrevious (bool setSelection)
		{
			return textEditorWidget.FindPrevious (setSelection);
		}
		
		public bool Replace (string withPattern)
		{
			return textEditorWidget.Replace (withPattern);
		}
		
		public int ReplaceAll (string withPattern)
		{
			return textEditorWidget.ReplaceAll (withPattern);
		}
		#endregion

		#region Coordinate transformation
		public DocumentLocation PointToLocation (double xp, double yp)
		{
			return TextViewMargin.PointToLocation (xp, yp);
		}
		
		public DocumentLocation PointToLocation (Cairo.Point p)
		{
			return TextViewMargin.PointToLocation (p);
		}
		
		public DocumentLocation PointToLocation (Cairo.PointD p)
		{
			return TextViewMargin.PointToLocation (p);
		}
		
		public Cairo.Point LocationToPoint (DocumentLocation loc)
		{
			return TextViewMargin.LocationToPoint (loc);
		}
		
		public Cairo.Point LocationToPoint (int line, int column)
		{
			return TextViewMargin.LocationToPoint (line, column);
		}
		
		public Cairo.Point LocationToPoint (int line, int column, bool useAbsoluteCoordinates)
		{
			return TextViewMargin.LocationToPoint (line, column, useAbsoluteCoordinates);
		}
		
		public Cairo.Point LocationToPoint (DocumentLocation loc, bool useAbsoluteCoordinates)
		{
			return TextViewMargin.LocationToPoint (loc, useAbsoluteCoordinates);
		}
		
		public double ColumnToX (DocumentLine line, int column)
		{
			return TextViewMargin.ColumnToX (line, column);
		}
		
		/// <summary>
		/// Calculates the line number at line start (in one visual line could be several logical lines be displayed).
		/// </summary>
		public int YToLine (double yPos)
		{
			return TextViewMargin.YToLine (yPos);
		}
		
		public double LineToY (int logicalLine)
		{
			return TextViewMargin.LineToY (logicalLine);
		}
		
		public double GetLineHeight (DocumentLine line)
		{
			return TextViewMargin.GetLineHeight (line);
		}
		
		public double GetLineHeight (int logicalLineNumber)
		{
			return TextViewMargin.GetLineHeight (logicalLineNumber);
		}
		#endregion


		public void SetCaretTo (int line, int column)
		{
			textEditorWidget.SetCaretTo (line, column);
		}
		
		public void SetCaretTo (int line, int column, bool highlight)
		{
			textEditorWidget.SetCaretTo (line, column, highlight);
		}
		
		public void SetCaretTo (int line, int column, bool highlight, bool centerCaret)
		{
			textEditorWidget.SetCaretTo (line, column, highlight, centerCaret);
		}
	}
}
