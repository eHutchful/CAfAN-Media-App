using Android.Views;
using Android.Widget;

namespace DivineVerITies.Helpers
{
    public interface ISimpleMediaHolder
    {
        TextView Album { get; set; }
        ImageView AlbumArt { get; set; }
        View mainView { get; set; }
        ImageButton Options { get; set; }
        TextView Title { get; set; }
    }
}