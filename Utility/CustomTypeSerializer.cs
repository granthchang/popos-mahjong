using CardUtilities;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CustomTypeSerializer : MonoBehaviour {
  void Start() {
    PhotonPeer.RegisterType(typeof(List<Player>), 100, SerializePlayerList, DeserializePlayerList);
    PhotonPeer.RegisterType(typeof(CardUtilities.Card), 101, SerializeCard, DeserializeCard);
    PhotonPeer.RegisterType(typeof(List<CardUtilities.Card>), 102, SerializeCardList, DeserializeCardList);
  }

  private static byte[] JoinBytes(byte[] leftBytes, byte[] rightBytes) {
    byte[] rv = new byte[leftBytes.Length + rightBytes.Length];
    System.Buffer.BlockCopy(leftBytes, 0, rv, 0, leftBytes.Length);
    System.Buffer.BlockCopy(rightBytes, 0, rv, leftBytes.Length, rightBytes.Length);
    return rv;
  }

  private static byte[] SerializePlayerList(object customobject) {
    List<Player> list = (List<Player>)customobject;
    byte[] allBytes = new byte[0];
    foreach (Player p in list) {
      byte[] playerBytes = BitConverter.GetBytes(p.ActorNumber);
      if (BitConverter.IsLittleEndian) {
        Array.Reverse(playerBytes);
      }
      allBytes = JoinBytes(allBytes, playerBytes);
    }
    return allBytes;
  }

  private static object DeserializePlayerList(byte[] bytes) {
    List<Player> list = new List<Player>();
    for (int i = 0; i < bytes.Length / 4; i++) {
      // Parse bytes
      byte[] playerBytes = new byte[4];
      System.Buffer.BlockCopy(bytes, i * 4, playerBytes, 0, 4);
      if (BitConverter.IsLittleEndian) {
        Array.Reverse(playerBytes);
      }
      int ID = BitConverter.ToInt32(playerBytes, 0);
      // Find and add player
      if (PhotonNetwork.CurrentRoom != null) {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(ID);
        list.Add(player);
      }
    }
    return list;
  }

  private static byte[] SerializeCard(object customobject) {
    CardUtilities.Card card = (CardUtilities.Card)customobject;
    byte[] bytes = BitConverter.GetBytes(card.ID);
    if (BitConverter.IsLittleEndian) {
      Array.Reverse(bytes);
    }
    return bytes;
  }

  private static object DeserializeCard(byte[] bytes) {
    byte[] cardBytes = new byte[4];
    System.Buffer.BlockCopy(bytes, 0, cardBytes, 0, 4);
    if (BitConverter.IsLittleEndian) {
      Array.Reverse(cardBytes);
    }
    int ID = BitConverter.ToInt32(cardBytes, 0);
    return new CardUtilities.Card(ID);
  }

  private static byte[] SerializeCardList(object customobject) {
    List<CardUtilities.Card> list = (List<CardUtilities.Card>)customobject;
    byte[] allBytes = new byte[0];
    foreach (CardUtilities.Card c in list) {
      byte[] cardBytes = BitConverter.GetBytes(c.ID);
      if (BitConverter.IsLittleEndian) {
        Array.Reverse(cardBytes);
      }
      allBytes = JoinBytes(allBytes, cardBytes);
    }
    return allBytes;
  }

  private static object DeserializeCardList(byte[] bytes) {
    List<CardUtilities.Card> list = new List<CardUtilities.Card>();
    for (int i = 0; i < bytes.Length / 4; i++) {
      // Parse bytes
      byte[] cardBytes = new byte[4];
      System.Buffer.BlockCopy(bytes, i * 4, cardBytes, 0, 4);
      if (BitConverter.IsLittleEndian) {
        Array.Reverse(cardBytes);
      }
      int ID = BitConverter.ToInt32(cardBytes, 0);
      // Add new card to list
      list.Add(new Card(ID));
    }
    return list;
  }
}