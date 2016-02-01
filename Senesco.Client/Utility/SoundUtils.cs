using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Media;
using log4net;
using Senesco.Client.Sound;

namespace Senesco.Client.Utility
{
   public class SoundUtils
   {
      private static readonly ILog s_log = LogManager.GetLogger(typeof(SoundUtils));

      // A lookup of sound players by name.
      private static Dictionary<SoundEffect, SoundPlayer> s_soundLookup = new Dictionary<SoundEffect, SoundPlayer>();

      /// <summary>
      /// Plays the given sound by name.
      /// </summary>
      /// <param name="soundName">The name of the sound to play.</param>
      /// <returns>True upon error, false otherwise.</returns>
      public static Status PlaySound(SoundEffect soundName)
      {
         // Look up the sound by its key.
         SoundPlayer player;
         if (s_soundLookup.TryGetValue(soundName, out player) == false)
         {
            s_log.WarnFormat("No sound loaded for key {0}", soundName);
            return Status.Success;
         }

         if (player == null)
         {
            s_log.ErrorFormat("No SoundPlayer for key {0}", soundName);
            return Status.NoResult;
         }

         try
         {
            player.Play();
            return Status.Success;
         }
         catch (Exception e)
         {
            s_log.ErrorFormat("Failed to play sound file {0}: {1}", player.SoundLocation, e.Message);
            return Status.Failure;
         }
      }

      /// <summary>
      /// Deletes all loaded sounds.
      /// </summary>
      public static void ClearSounds()
      {
         s_soundLookup.Clear();
      }

      /// <summary>
      /// Adds or replaces a sound by name and file path.
      /// </summary>
      /// <param name="soundName">The name of the sound to load.</param>
      /// <param name="filePath">The path of the sound file.</param>
      /// <returns>True upon error, false otherwise.</returns>
      public static Status SetSound(SoundEffect soundName, string filePath)
      {
         // If the path is bad, 
         if (String.IsNullOrEmpty(filePath) == true || File.Exists(filePath) == false)
         {
            s_log.ErrorFormat("Bad path provided: {0}", filePath);
            return Status.Failure;
         }

         // Make the SoundPlayer and save the sound name as the Tag.
         SoundPlayer player = new SoundPlayer(filePath);
         player.Tag = soundName;

         // Start loading the file.
         s_log.DebugFormat("Starting load of sound {0}.", player.SoundLocation);
         player.LoadCompleted += SoundLoadComplete;
         player.LoadAsync();

         return Status.Success;
      }

      private static void SoundLoadComplete(object sender, AsyncCompletedEventArgs e)
      {
         // Get the SoundPlayer as the sender.
         SoundPlayer player = sender as SoundPlayer;
         if (player == null)
         {
            s_log.Error("Bad sender in SoundLoadComplete()");
            return;
         }
         
         // Log any asynchronous loading failures.
         if (e.Cancelled == true)
         {
            s_log.ErrorFormat("Loading of {0} was cancelled.", player.SoundLocation);
            return;
         }
         
         if (e.Error != null)
         {
            s_log.ErrorFormat("Error loading {0}:", player.SoundLocation, e.Error.Message);
            return;
         }

         // Note when completed loading.
         s_log.DebugFormat("Loading of sound {0} is complete.", player.SoundLocation);

         // Store the ready-to-play SoundPlayer in the sound lookup.
         s_soundLookup[(SoundEffect)player.Tag] = player;
      }
   }
}
