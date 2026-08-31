using Cysharp.Threading.Tasks;
using UnityEngine;

public abstract class ViewTransition : MonoBehaviour
{
    public abstract UniTask PlayEnterAsync();
    public abstract UniTask PlayExitAsync();
}
