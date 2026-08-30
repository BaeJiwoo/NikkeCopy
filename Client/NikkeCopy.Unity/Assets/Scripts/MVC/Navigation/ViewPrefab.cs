using UnityEngine;

namespace NikkeCopy.Client.MVC.Navigation
{
    public sealed class ViewPrefab : MonoBehaviour
    {
        [SerializeField] private ViewKey key;
        public ViewKey Key => key;
    }
}
