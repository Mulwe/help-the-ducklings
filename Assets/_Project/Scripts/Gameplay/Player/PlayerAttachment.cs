using UnityEngine;

public class PlayerAttachment : MonoBehaviour
{
    [Header("Follower:")]
    public Transform followChild;
    public bool hasFollower;

    [Header("Last-In-Line:")]
    public Transform lastfollowChild;
    private DuckController _lastDC;

    [Header("Collision:")]
    private Collider2D _collider;
    private PlayerAudio _playerAudio;


    public void UpdateLinq()
    {
        if (followChild == null)
        {
            hasFollower = false;
            lastfollowChild = null;
            _lastDC = null;
        }
    }

    //first duck in Line
    public void CreateFollowChild(Transform followingChild)
    {
        this.followChild = followingChild;
        hasFollower = true;
        UpdateLastInLine(followingChild);
    }

    public void UpdateLastInLine(Transform lastInLine)
    {
        this.lastfollowChild = lastInLine;
        if (lastInLine != null)
            _lastDC = lastInLine.GetComponent<DuckController>();
        else
            _lastDC = null;
    }

    public Transform DetachLastInLine()
    {
        if (_lastDC != null)
        {
            DuckController targetDC = _lastDC;

            //change behaivior of last connected child
            targetDC.CleanDuckFollower();

            //Does duck follow someone?
            if (_lastDC.GetCurrentParent() == null)
            {
                //no parent => detach and make it valid to connect 
                targetDC.CancelIsAirbornAndSavePosition();
                UpdateLastInLine(null);
                UpdateLinq();
                return null;
            }

            //попытка достать родителя из последней в очереди утки
            //followParent is a duck
            if (_lastDC.GetCurrentParent().TryGetComponent<DuckController>(out var secondToLastInLine))
            {
                // secondToLastInLine => become last in line
                secondToLastInLine.UpdateParentChildRelation(true, secondToLastInLine.GetCurrentParent(), false, null);

                //update new lastfollowChild & _lastDc
                UpdateLastInLine(secondToLastInLine.transform);

                // detach old targetDС
                targetDC.CancelIsAirbornAndSavePosition();

            }
            else
            {
                //followParent is a player
                //prepare target-duck to detach
                // disconnect followparent 
                targetDC.CancelIsAirbornAndSavePosition();


                //last in line for player is null
                UpdateLastInLine(null);

                //clean players 
                PlayerAttachmentsUpdate(false, null, null);
            }
            return (targetDC.transform);
        }
        return null;
    }

    public void DetachWholeLink()
    {
        UpdateLinq();
        if (lastfollowChild != null || followChild != null)
        {
            while (followChild != null)
            {

                if (lastfollowChild == followChild)
                {
                    Debug.Log("Last in line: lastfollowChild == followChild");
                    DetachLastInLine();
                    followChild = null;
                    lastfollowChild = null;
                    UpdateLinq();
                    break;
                }
                Transform tmp = DetachLastInLine();
                if (tmp == null)
                {
                    Debug.Log($"{nameof(DetachLastInLine)} is null");
                    break;
                }
            }
        }
    }
    private void Awake()
    {
        followChild = null;
        hasFollower = false;
        lastfollowChild = null;
        _lastDC = null;
        UpdateLinq();
        _collider = GetComponent<Collider2D>();
        _playerAudio = this.GetComponent<PlayerAudio>();
    }
    private void PlayerAttachmentsUpdate(bool hasFollower, Transform followChild, Transform lastInLine)
    {
        this.hasFollower = hasFollower;
        this.followChild = followChild;
        UpdateLastInLine(lastInLine);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision == null) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            if (hasFollower)
            {
                Debug.Log("Duckling left the party!");
                this.DetachLastInLine();
            }
        }
    }
}
