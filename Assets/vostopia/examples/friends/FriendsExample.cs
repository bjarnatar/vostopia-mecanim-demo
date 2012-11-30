using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FriendsExample : MonoBehaviour
{
    public GameObject AvatarControllerPrefab;

    private List<VostopiaUser> Friends;
    private List<VostopiaFriendRequest> FriendRequests;
    private List<VostopiaRecentlyMet> RecentlyMet;

    private int CurrentSpawnPosition;
    private string AddUserUsername = "";
    private string AddUserMessage;

    void Start()
    {
        //Check that the avatar controller is set and has the AvatarLoaderUserId component
        if (!AvatarControllerPrefab || AvatarControllerPrefab.GetComponent<AvatarLoaderUserId>() == null)
        {
            Debug.LogError("Please set AvatarController to a game object with the AvatarLoaderUserId component");
            return;
        }

        StartCoroutine(LoadFriends());
        StartCoroutine(LoadFriendRequests());
        StartCoroutine(LoadRecentlyMet());
    }

    void Spawn(string userId)
    {
        GameObject friendAvatar = (GameObject)GameObject.Instantiate(AvatarControllerPrefab);
        AvatarLoaderUserId friendLoader = friendAvatar.GetComponent<AvatarLoaderUserId>();
        friendLoader.UserId = userId;
        friendAvatar.transform.position = new Vector3((CurrentSpawnPosition + 1) * 1f, transform.position.y, transform.position.z);
        CurrentSpawnPosition++;
    }

    IEnumerator LoadFriends()
    {
        while (!VostopiaClient.IsAuthenticated)
        {
            yield return null;
        }

        //list friends
        Debug.Log("Finding friends of " + VostopiaClient.Authentication.ActiveUser.DisplayName);
        ApiCall call = VostopiaClient.Friends.BeginList();
        IEnumerator e = call.Wait();
        while (e.MoveNext()) yield return e.Current;
        Friends = VostopiaClient.Friends.EndList(call);
        Debug.Log(string.Format("Found {0} friends", Friends.Count));
    }

    IEnumerator LoadFriendRequests()
    {
        while (!VostopiaClient.IsAuthenticated)
        {
            yield return null;
        }

        //list friend requests
        Debug.Log("Finding open friends requests");
        ApiCall call = VostopiaClient.Friends.BeginListRequests();
        IEnumerator e = call.Wait();
        while (e.MoveNext()) yield return e.Current;
        FriendRequests = VostopiaClient.Friends.EndListRequests(call);
        Debug.Log(string.Format("Found {0} friend requests", FriendRequests.Count));
    }

    IEnumerator LoadRecentlyMet()
    {
        while (!VostopiaClient.IsAuthenticated)
        {
            yield return null;
        }

        //list recently met
        Debug.Log("Finding recently met");
        ApiCall call = VostopiaClient.Friends.BeginListRecentlyMet();
        IEnumerator e = call.Wait();
        while (e.MoveNext()) yield return e.Current;
        RecentlyMet = VostopiaClient.Friends.EndListRecentlyMet(call);
        Debug.Log(string.Format("Found {0} recently met", RecentlyMet.Count));
    }

    IEnumerator UpdateFriendsRecentlyMet()
    {
        if (Friends == null)
        {
            yield break;
        }

        List<string> friendIds = Friends.Select(friend => friend.Id).ToList();
        ApiCall call = VostopiaClient.Friends.BeginUpdateRecentlyMet(friendIds);
        IEnumerator e = call.Wait();
        while (e.MoveNext()) yield return e.Current;
        VostopiaClient.Friends.EndUpdateRecentlyMet(call);
    }

    IEnumerator SendFriendRequest(string username)
    {
        while (!VostopiaClient.IsAuthenticated)
        {
            yield return null;
        }

        ApiCall call = VostopiaClient.Friends.BeginRequestFriendship(username);
        IEnumerator e = call.Wait();
        while (e.MoveNext()) yield return e.Current;
        VostopiaFriendRequest request = VostopiaClient.Friends.EndRequestFriendship(call);
        if (request != null)
        {
            if (request.Status == VostopiaFriendRequestStatus.PENDING_OUTBOUND)
            {
                AddUserMessage = "Sent friend request to " + request.Friend.DisplayName;
            }
            else if (request.Status == VostopiaFriendRequestStatus.ACCEPTED)
            {
                AddUserMessage = "Added " + request.Friend.DisplayName + " to friends list";
            }
            else
            {
                AddUserMessage = "hm?";
            }
        }
        else
        {
            AddUserMessage = "Could not find user";
        }

        Invoke("ClearAddUserMessage", 3);
    }

    IEnumerator AcceptFriendship(string friendUserId)
    {
        while (!VostopiaClient.IsAuthenticated)
        {
            yield return null;
        }

        var call = VostopiaClient.Friends.BeginAcceptFriendshipRequest(friendUserId);
        var e = call.Wait();
        while (e.MoveNext()) { yield return e.Current; }
        if (VostopiaClient.Friends.EndAcceptFriendshipRequest(call))
        {
            //Refresh Friends and Requests
            StartCoroutine(LoadFriends());
            if (FriendRequests != null)
            {
                FriendRequests.RemoveAll(req => req.Friend.Id.ToString() == friendUserId);
            }
        }
        else
        {
            StartCoroutine(LoadFriendRequests());
        }
    }

    IEnumerator DeclineFriendship(string friendUserId)
    {
        while (!VostopiaClient.IsAuthenticated)
        {
            yield return null;
        }

        var call = VostopiaClient.Friends.BeginDeclineFriendshipRequest(friendUserId);
        var e = call.Wait();
        while (e.MoveNext()) { yield return e.Current; }
        if (VostopiaClient.Friends.EndDeclineFriendshipRequest(call))
        {
            if (FriendRequests != null)
            {
                FriendRequests.RemoveAll(req => req.Friend.Id.ToString() == friendUserId);
            }
        }
        else
        {
            Debug.Log("Unable to decline friendship " + call.Error);
            StartCoroutine(LoadFriendRequests());
        }
    }

    void ClearAddUserMessage()
    {
        AddUserMessage = null;
    }

    void OnGUI()
    {
		if (!VostopiaClient.IsAuthenticated)
		{
			return;
		}

        GUILayout.BeginArea(new Rect(10, 10, 300, 400));

		GUILayout.Label("This example shows how to retrieve the list of vostopia friends, send friend requests, and accept/reject friend requests");

        GUILayout.Label("Username/UserID/Email");
        AddUserUsername = GUILayout.TextField(AddUserUsername);
        if (GUILayout.Button("Send Friend Request"))
        {
            StartCoroutine(SendFriendRequest(AddUserUsername));
        }
        if (AddUserMessage != null)
        {
            GUILayout.Label(AddUserMessage);
        }

        if (Friends != null)
        {
            GUILayout.Label("Friends:");
            foreach (VostopiaUser friend in Friends)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Spawn"))
                {
                    Spawn(friend.Id);
                }
                GUILayout.Label(friend.DisplayName);
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("Post Recently Met Friends"))
            {
                StartCoroutine(UpdateFriendsRecentlyMet());
            }
        }

        if (FriendRequests != null)
        {
            GUILayout.Label("Friend Requests:");
            foreach (VostopiaFriendRequest request in FriendRequests)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Spawn"))
                {
                    Spawn(request.Friend.Id);
                }
                if (request.Status == VostopiaFriendRequestStatus.PENDING_INBOUND)
                {
                    if (GUILayout.Button("Accept"))
                    {
                        StartCoroutine(AcceptFriendship(request.Friend.Id));
                    }
                    if (GUILayout.Button("Decline"))
                    {
                        StartCoroutine(DeclineFriendship((request.Friend.Id)));
                    }
                }
                else if (request.Status == VostopiaFriendRequestStatus.PENDING_OUTBOUND)
                {
                    if (GUILayout.Button("Cancel"))
                    {
                        StartCoroutine(DeclineFriendship((request.Friend.Id)));
                    }
                }

                GUILayout.Label(request.Friend.DisplayName);
                GUILayout.Label(request.CreationDate.ToLocalTime().ToString());
                GUILayout.EndHorizontal();
            }
        }

        if (RecentlyMet != null)
        {
            GUILayout.Label("Recently Met:");
            foreach (VostopiaRecentlyMet meet in RecentlyMet)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Spawn"))
                {
                    Spawn(meet.User.Id);
                }
                GUILayout.Label(meet.User.DisplayName);
                GUILayout.Label(meet.Date.ToLocalTime().ToString());
                GUILayout.EndHorizontal();
            }
        }

        GUILayout.EndArea();
    }

}
