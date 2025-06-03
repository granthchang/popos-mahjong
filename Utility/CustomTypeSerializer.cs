using CardUtilities;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomTypeSerializer : MonoBehaviour {
  void Start() {
    PhotonPeer.RegisterType(typeof(List<Player>), 100, SerializePlayerList, DeserializePlayerList);
    PhotonPeer.RegisterType(typeof(CardUtilities.Card), 101, SerializeCard, DeserializeCard);
    PhotonPeer.RegisterType(typeof(List<CardUtilities.Card>), 102, SerializeCardList, DeserializeCardList);
    PhotonPeer.RegisterType(typeof(LockableWrapper), 103, SerializeLockableWrapper, DeserializeLockableWrapper);
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

  private static byte[] SerializeLockableWrapper(object customobject) {
    LockableWrapper wrapper = (LockableWrapper)customobject;
    byte[] allBytes = new byte[0];
    // Serialize discard
    byte[] discardBytes = BitConverter.GetBytes(wrapper.Discard != null ? wrapper.Discard.ID : 0);
    if (BitConverter.IsLittleEndian) {
      Array.Reverse(discardBytes);
    }
    allBytes = JoinBytes(allBytes, discardBytes);
    // Serialize each set
    foreach (Set set in wrapper.Sets) {
      // Serialize set type
      byte[] typeBytes = BitConverter.GetBytes((int)set.Type);
      if (BitConverter.IsLittleEndian) {
        Array.Reverse(typeBytes);
      }
      // Serialize number of cards in set
      byte[] countBytes = BitConverter.GetBytes(set.Cards.Count);
      if (BitConverter.IsLittleEndian) {
        Array.Reverse(countBytes);
      }
      // Add all bytes to the array
      allBytes = JoinBytes(allBytes, typeBytes);
      allBytes = JoinBytes(allBytes, countBytes);
      allBytes = JoinBytes(allBytes, SerializeCardList(set.Cards));
    }
    return allBytes;
  }

  private static object DeserializeLockableWrapper(byte[] bytes) {
    // Deserialize discard
    byte[] discardBytes = new byte[4];
    System.Buffer.BlockCopy(bytes, 0, discardBytes, 0, 4);
    if (BitConverter.IsLittleEndian) {
      Array.Reverse(discardBytes);
    }
    int discardId = BitConverter.ToInt32(discardBytes, 0);
    Card discard = new Card(discardId);

    // Deserialize sets
    List<Set> sets = new List<Set>();
    int i = 4;
    while (i < bytes.Length) {
      // Parse first 4 bytes to int for set type
      byte[] typeBytes = new byte[4];
      Buffer.BlockCopy(bytes, i, typeBytes, 0, 4);
      if (BitConverter.IsLittleEndian) {
        Array.Reverse(typeBytes);
      }
      SetType type = (SetType)BitConverter.ToInt32(typeBytes, 0);

      // Parse next 4 bytes for number of cards to parse
      byte[] countBytes = new byte[4];
      Buffer.BlockCopy(bytes, i + 4, countBytes, 0, 4);
      if (BitConverter.IsLittleEndian) {
        Array.Reverse(countBytes);
      }
      int count = BitConverter.ToInt32(countBytes, 0);

      // Parse next count*4 bytes to get the list of cards in the set 
      byte[] cardBytes = new byte[count * 4];
      Buffer.BlockCopy(bytes, i + 8, cardBytes, 0, count * 4);
      List<Card> cardList = (List<Card>)DeserializeCardList(cardBytes);

      // Add set to list
      sets.Add(new Set(type, cardList));

      // Increment index for next set
      i += (8 + count * 4);
    }
    return new LockableWrapper(sets, discard);
  }
}