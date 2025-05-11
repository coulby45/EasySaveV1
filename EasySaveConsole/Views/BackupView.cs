using System;
using System.Collections.Generic;
using EasySaveConsole.Models;

namespace EasySaveConsole.Views
{
    public class BackupView
    {
        public string ChooseLanguage()
        {
            Console.WriteLine("1. Français");
            Console.WriteLine("2. English");
            Console.Write("> ");
            return Console.ReadLine() == "1" ? "fr" : "en";
        }

        public string DisplayMenu(string lang)
        {
            if (lang == "fr")
            {
                Console.WriteLine("=== Menu EasySave ===");
                Console.WriteLine("1. Créer une sauvegarde");
                Console.WriteLine("2. Afficher les sauvegardes");
                Console.WriteLine("3. Modifier une sauvegarde");
                Console.WriteLine("4. Supprimer une sauvegarde");
                Console.WriteLine("5. Afficher les logs");
                Console.WriteLine("6. Quitter");
            }
            else
            {
                Console.WriteLine("=== EasySave Menu ===");
                Console.WriteLine("1. Create backup");
                Console.WriteLine("2. Display backups");
                Console.WriteLine("3. Update backup");
                Console.WriteLine("4. Delete backup");
                Console.WriteLine("5. Show logs");
                Console.WriteLine("6. Exit");
            }
            Console.Write("> ");
            return Console.ReadLine();
        }

        public Backup AskNewBackupInfo(string lang, string existingName = null)
        {
            Console.Write((lang == "fr" ? "Nom" : "Name")
                          + (existingName != null ? $" ({existingName})" : "") + ": ");
            var name = existingName ?? Console.ReadLine();
            Console.Write((lang == "fr" ? "Chemin source" : "Source path") + ": ");
            var src = Console.ReadLine();
            Console.Write((lang == "fr" ? "Chemin cible" : "Target path") + ": ");
            var dst = Console.ReadLine();
            Console.Write((lang == "fr" ? "Type (Full/Differential)" : "Type (Full/Differential)") + ": ");
            var typ = Console.ReadLine();
            return new Backup
            {
                Name = name,
                SourcePath = src,
                TargetPath = dst,
                Type = typ,
                FileLength = 0
            };
        }

        public void DisplayBackupList(IEnumerable<Backup> list)
        {
            Console.WriteLine("=== Liste des sauvegardes ===");
            int i = 1;
            foreach (var b in list)
                Console.WriteLine($"{i++}. {b}");
        }

        public bool ConfirmSearch(string lang)
        {
            Console.Write(lang == "fr"
                ? "Voulez-vous chercher par nom ? (o/n): "
                : "Search by name? (y/n): ");
            var c = Console.ReadLine()?.ToLower();
            return c == "o" || c == "y";
        }

        public string AskBackupName(string lang)
        {
            Console.Write(lang == "fr" ? "Nom de la sauvegarde: " : "Backup name: ");
            return Console.ReadLine();
        }

        public void DisplayBackup(Backup b)
        {
            Console.WriteLine(b);
        }

        public void ShowMessage(string code, string lang)
        {
            var msg = code switch
            {
                "max_jobs" => lang == "fr"
                    ? "Nombre maximum de jobs atteint."
                    : "Maximum number of jobs reached.",
                "not_found" => lang == "fr"
                    ? "Sauvegarde introuvable."
                    : "Backup not found.",
                "invalid" => lang == "fr"
                    ? "Option invalide."
                    : "Invalid option.",
                _ => ""
            };
            Console.WriteLine(msg);
        }
    }
}
