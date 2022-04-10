// TeleportPad
// a Valheim mod skeleton using Jötunn
// 
// File:    TeleportPad.cs
// Project: TeleportPad

using BepInEx;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Logger = Jotunn.Logger;

namespace TeleportPad
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class TeleportPad : BaseUnityPlugin
    {
        public const string PluginGUID = "pensiboto.TeleportPad";
        public const string PluginName = "TeleportPad";
        public const string PluginVersion = "0.0.1";

        private Texture2D testTex;
        private Sprite testSprite;

        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private void Awake()
        {
            PrefabManager.OnVanillaPrefabsAvailable += AddPieceCategories;
        }

        private void Update()
        {
            foreach (Piece piece in Piece.m_allPieces)
            {
                if (isPlayerOnTopOfPad(Player.m_localPlayer))
                {
                    Vector3 offset = new Vector3(2,2);
                    Vector3 pos = Camera.main.transform.position + offset;
                    Quaternion rot = Quaternion.identity;
                    bool distantTeleport = false;
                    Player.m_localPlayer.TeleportTo(pos, rot, distantTeleport);
                }
            }
        }

        private bool isPlayerOnTopOfPad(Player localPlayer)
        {
            Collider[] piecesColliders = Physics.OverlapSphere(localPlayer.transform.position, .05f, LayerMask.GetMask("piece", "piece_nonsolid"));
            List<Piece> pieces = new List<Piece>();

            foreach (Collider piecesCollider in piecesColliders)
            {
                Piece piece = piecesCollider.GetComponentInParent<Piece>();
                if (piece != null)
                {
                    pieces.Add(piece);
                }
            }

            Vector3 playerPoint = localPlayer.GetCenterPoint();
            List<Piece> orderedPieces = pieces.OrderBy(p => Vector3.Distance(playerPoint, p.transform.position)).ToList();

            foreach (Piece piece in orderedPieces)
            {
                if (piece.m_name == "Teleport Pad" && playerPoint.y > piece.transform.position.y)
                {
                    return true;  
                }
            }
            return false;
        }
        // Implementation of custom pieces from an "empty" prefab with new piece categories
        private void AddPieceCategories()
        {
            try
            {
                // Create a new CustomPiece as an "empty" GameObject. Also set addZNetView to true 
                // so it will be saved and shared with all clients of a server.
                CustomPiece CP = new CustomPiece("TeleportPad", "stone_floor_2x2", 
                    new PieceConfig
                    {
                        Name = "Teleport Pad",
                        Description = "test description",
                        PieceTable = "Hammer",
                        Category = "Crafting" 
                    });

                if (CP != null)
                {
                    // Add our test texture to the Unity MeshRenderer
                    //var prefab = CP.PiecePrefab;
                    //prefab.GetComponent<MeshRenderer>().material.color = Color.red;

                    PieceManager.Instance.AddPiece(CP);
                }
            }
            catch (Exception ex)
            {
                Jotunn.Logger.LogError($"Error while adding cloned item: {ex.Message}");
            }
            finally
            {
                // You want that to run only once, Jotunn has the item cached for the game session
                PrefabManager.OnVanillaPrefabsAvailable -= AddPieceCategories;
            }
        }
    }
}

