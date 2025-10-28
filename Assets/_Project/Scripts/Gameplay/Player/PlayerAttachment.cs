using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttachment : MonoBehaviour
{
    [Header("Follower:")]
    public Transform followChild;
    public bool hasFollower;

    [Header("Last-In-Line:")]
    public Transform lastFollowChild;
    private DuckController _lastDC;

    [Header("Collision:")]
    private Collider2D _collider;
    private PlayerAudio _playerAudio;

    [Tooltip("Debug")]
    public int amount = 0;
    private bool _isLogging = false;

    public void UpdateLink()
    {
        if (followChild == null)
        {
            hasFollower = false;
            UpdateLastInLine(null);
        }
    }

    //first duck in Line
    public void CreateFollowChild(Transform followingChild)
    {
        //Debug.Log("Create first follow child");

        this.followChild = followingChild;
        hasFollower = true;
        UpdateLastInLine(followingChild);
    }

    public void UpdateLastInLine(Transform lastInLine)
    {
        this.lastFollowChild = lastInLine;
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

            //Does last-in-line duck follow someone?
            if (_lastDC.GetCurrentParent() == null)
            {
                //Duck doesn't have parent => detach and make it valid to connect
                targetDC.CleanDuckParent();

                lastFollowChild = null;
                _lastDC = null;
                hasFollower = false;
                followChild = null;
                return null;
            }

            // Try get Parent of last-in-line
            if (_lastDC.GetCurrentParent().gameObject.CompareTag("Duck") &&
                _lastDC.GetCurrentParent().TryGetComponent<DuckController>(out var parentDuck))
            {
                // Parent last-in-line is a Duck.
                // Make last-in-line Parent into new last-in-line Duck.
                // Update Reationship
                parentDuck.UpdateParentChildRelationship(true, parentDuck.GetCurrentParent(), false, null);

                // Update & Cache references of last-in-line
                UpdateLastInLine(parentDuck.transform);

                // Detach old last-in-line. Clean references
                targetDC.CleanDuckParent();
                targetDC.CleanDuckChild();
            }
            else
            {
                // Parent last-in-line is a Player (followParent)

                // Disconnect/Detach duck (targetDC) from Player()
                targetDC.CleanDuckParent();
                targetDC.CleanDuckChild();

                // Clean last-in-line for player
                UpdateLastInLine(null);

                // Clean Player's references
                PlayerAttachmentsUpdate(false, null, null);
            }
            return (targetDC.transform);
        }
        return null;
    }

    public void DetachWholeLink()
    {
        UpdateLink();
        if (lastFollowChild != null || followChild != null)
        {
            while (followChild != null)
            {
                if (lastFollowChild == followChild)
                {
                    Debug.Log("Last in line: lastfollowChild == followChild");
                    DetachLastInLine();
                    followChild = null;
                    lastFollowChild = null;
                    UpdateLink();
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
        lastFollowChild = null;
        _lastDC = null;
        UpdateLink();
        _collider = GetComponent<Collider2D>();
        _playerAudio = this.GetComponent<PlayerAudio>();
        StartCoroutine(InitializePlayerInput());
    }

    private IEnumerator InitializePlayerInput()
    {
        PlayerInput playerInput = GetComponent<PlayerInput>();
        while (playerInput == null)
        {
            playerInput = GetComponent<PlayerInput>();
            yield return null;
        }

        playerInput.actions.Enable();
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
                this.DetachLastInLine();
                //amount = TrackChain();
                //Debug.Log($"Duckling left the party! {amount} in chain");
            }
        }
    }

    private int TrackChain()
    {
        int i = 0;
        if (lastFollowChild != null)
        {
            if (_lastDC != null && _lastDC.GetCurrentParent() != null)
            {
                i++;
                Log($"{i}. Last Duck — Parent:{_lastDC.GetCurrentParent()}." +
                    $" Child: {_lastDC.GetCurrentChild()}. Has Follower: {_lastDC.HasFollower}");
                Transform nextInLine = _lastDC.GetCurrentParent();

                while (nextInLine != null)
                {
                    if (nextInLine.CompareTag("Duck"))
                    {
                        i++;
                        var dc = nextInLine.GetComponent<DuckController>();

                        Log(
                            $"{i} Duck in line — Parent:{dc.GetCurrentParent()}." +
                            $" Child: {dc.GetCurrentChild()}. Has Follower: {dc.HasFollower}");
                        nextInLine = dc.GetCurrentParent();
                    }
                    else if (nextInLine.CompareTag("Player") || nextInLine.gameObject == this.gameObject)
                    {
                        //player
                        Log(
                            $" Player in line — Parent: none" +
                            $" Child: {this.followChild}. Has Follower: {this.hasFollower}");
                        break;
                    }
                }
            }
        }
        return i;
    }

    private void Log(string msg)
    {
        if (_isLogging)
            Debug.Log(msg);
    }
}