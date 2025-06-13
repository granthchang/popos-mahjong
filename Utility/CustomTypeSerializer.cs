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
    PhotonPeer.RegisterType(typeof(Card), 101, SerializeCard, DeserializeCard);
    PhotonPeer.RegisterType(typeof(List<Card>), 102, SerializeCardList, DeserializeCardList);
    PhotonPeer.RegisterType(typeof(LockableWrapper), 103, SerializeLockableWrapper, DeserializeLockableWrapper);
  }

  private static byte[] JoinBytes(byte[] leftBytes, byte[] rightBytes) {
    byte[] rv = new byte[leftBytes.Length + rightBytes.Length];
    Buffer.BlockCopy(leftBytes, 0, rv, 0, leftBytes.Length);
    Buffer.BlockCopy(rightBytes, 0, rv, leftBytes.Length, rightBytes.Length);
    return rv;
  }

  private static byte[] ConvertIntToBytes(int n) {
    byte[] intBytes = BitConverter.GetBytes(n);
    if (BitConverter.IsLittleEndian) {
      Array.Reverse(intBytes);
    }
    return intBytes;
  }

  private static int ConvertBytesToInt(byte[] bytes, int srcIndexToParse) {
    byte[] byteBlock = new byte[4];
    Buffer.BlockCopy(bytes, srcIndexToParse, byteBlock, 0, 4);
    if (BitConverter.IsLittleEndian) {
      Array.Reverse(byteBlock);
    }
    return BitConverter.ToInt32(byteBlock, 0);
  }

  private static byte[] SerializePlayerList(object customobject) {
    List<Player> list = (List<Player>)customobject;
    byte[] allBytes = new byte[0];
    foreach (Player p in list) {
      allBytes = JoinBytes(allBytes, ConvertIntToBytes(p.ActorNumber));
    }
    return allBytes;
  }

  private static object DeserializePlayerList(byte[] bytes) {
    List<Player> list = new List<Player>();
    for (int i = 0; i < bytes.Length / 4; i++) {
      int ID = ConvertBytesToInt(bytes, i * 4);
      if (PhotonNetwork.CurrentRoom != null) {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(ID);
        list.Add(player);
      }
    }
    return list;
  }

  private static byte[] SerializeCard(object customobject) {
    return ConvertIntToBytes(((Card)customobject).ID);
  }

  private static object DeserializeCard(byte[] bytes) {
    return new Card(ConvertBytesToInt(bytes, 0));
  }

  private static byte[] SerializeCardList(object customobject) {
    List<Card> list = (List<Card>)customobject;
    byte[] allBytes = new byte[0];
    foreach (Card c in list) {
      allBytes = JoinBytes(allBytes, ConvertIntToBytes(c.ID));
    }
    return allBytes;
  }

  private static object DeserializeCardList(byte[] bytes) {
    List<Card> list = new List<Card>();
    for (int i = 0; i < bytes.Length / 4; i++) {
      int ID = ConvertBytesToInt(bytes, i * 4);
      list.Add(new Card(ID));
    }
    return list;
  }

  private static byte[] SerializeLockableWrapper(object customobject) {
    LockableWrapper wrapper = (LockableWrapper)customobject;

    // Serialize discard
    byte[] allBytes = ConvertIntToBytes(wrapper.Discard != null ? wrapper.Discard.ID : 0);

    // Serialize each set
    foreach (Set set in wrapper.Sets) {
      allBytes = JoinBytes(allBytes, ConvertIntToBytes((int)set.Type));
      allBytes = JoinBytes(allBytes, ConvertIntToBytes(set.Cards.Count));
      allBytes = JoinBytes(allBytes, SerializeCardList(set.Cards));
    }
    return allBytes;
  }

  private static object DeserializeLockableWrapper(byte[] bytes) {
    // Deserialize discard
    int discardId = ConvertBytesToInt(bytes, 0);
    Card discard = (discardId == 0) ? null : new Card(discardId);

    // Deserialize sets
    List<Set> sets = new List<Set>();
    int i = 4;
    while (i < bytes.Length) {
      // Parse first 4 bytes to int for set type
      SetType type = (SetType)ConvertBytesToInt(bytes, i);

      // Parse next 4 bytes for number of cards to parse
      int count = ConvertBytesToInt(bytes, i + 4);

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