using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TestResources
{
	static NetworkManager s_TestNetworkManager;
	public static NetworkManager TestNetworkManager()
	{
		if (s_TestNetworkManager != null)
			return s_TestNetworkManager;
		var networkManagerPrefab = Resources.Load<NetworkManager>("NetworkManager");
		return s_TestNetworkManager = GameObject.Instantiate(networkManagerPrefab);
	}
}
