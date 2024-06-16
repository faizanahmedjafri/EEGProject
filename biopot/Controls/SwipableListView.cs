using Xamarin.Forms;

namespace biopot.Controls
{
    public class SwipableListView : ListView
    {
        /// <inheritdoc/>
         protected override void SetupContent(Cell content, int index)
         {
             base.SetupContent(content, index);

             var cell = (ViewCell) content;

             PanGestureRecognizer panGestureRecognizer = new PanGestureRecognizer();
             panGestureRecognizer.PanUpdated += OnPanUpdated;
             cell.View.GestureRecognizers.Add(panGestureRecognizer);
         }

        /// <inheritdoc/>
        protected override void UnhookContent(Cell content)
        {
            base.UnhookContent(content);

            var cell = (ViewCell)content;

            cell.View.GestureRecognizers.Clear();
        }

        /// <summary>
        /// Closes the view to show initial one before swipe.
        /// </summary>
        public void CloseView(SwipableListItem aView)
        {
            aView.ShowInitialView();
        }

        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            var view = (SwipableListItem)sender;
            view.ShowSwippedView();

            System.Diagnostics.Debug.WriteLine("OnPanUpdated");
        }
    }
}
