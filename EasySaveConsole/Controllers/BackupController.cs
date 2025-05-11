using System;
using System.Collections.Generic;
using EasySaveConsole.Managers;
using EasySaveConsole.Models;
using EasySaveConsole.Views;

namespace EasySaveConsole.Controllers
{
    public class BackupController
    {
        private readonly BackupManager _manager;
        private readonly BackupView _view;
        private string _language = "en";

        public BackupController()
        {
            _manager = new BackupManager();
            _view = new BackupView();
        }

        public void Start(string[] args)
        {
            // Traiter les arguments de ligne de commande
            if (args.Length > 0)
            {
                // Si l'argument est "execute" ou ressemble à un indice, exécuter les jobs
                if (args[0].ToLower() == "execute" && args.Length > 1)
                {
                    var indices = ParseArgs(args[1]);
                    _manager.ExecuteJobsByIndices(indices);
                    return;
                }
                // Si l'argument ressemble à des indices (contient des chiffres ou des tirets/points-virgules)
                else if (args[0].Contains("-") || args[0].Contains(";") || int.TryParse(args[0], out _))
                {
                    var indices = ParseArgs(args[0]);
                    _manager.ExecuteJobsByIndices(indices);
                    return;
                }
                // Si l'argument est "create" et il y a assez d'arguments, créer un job
                else if (args[0].ToLower() == "create" && args.Length >= 5)
                {
                    var newBackup = new Backup
                    {
                        Name = args[1],
                        SourcePath = args[2],
                        TargetPath = args[3],
                        Type = args[4],
                        FileLength = 0
                    };
                    if (_manager.AddJob(newBackup))
                        Console.WriteLine($"Backup '{args[1]}' created successfully.");
                    else
                        Console.WriteLine("Failed to create backup. Maximum number of jobs may have been reached.");
                    return;
                }
                // Si l'argument est "update" et il y a assez d'arguments, mettre à jour un job
                else if (args[0].ToLower() == "update" && args.Length >= 5)
                {
                    var updatedBackup = new Backup
                    {
                        Name = args[1],
                        SourcePath = args[2],
                        TargetPath = args[3],
                        Type = args[4],
                        FileLength = 0
                    };
                    if (_manager.UpdateJob(args[1], updatedBackup))
                        Console.WriteLine($"Backup '{args[1]}' updated successfully.");
                    else
                        Console.WriteLine($"Failed to update backup. Backup '{args[1]}' not found.");
                    return;
                }
                // Si l'argument est "delete" et il y a assez d'arguments, supprimer un job
                else if (args[0].ToLower() == "delete" && args.Length >= 2)
                {
                    if (_manager.RemoveJob(args[1]))
                        Console.WriteLine($"Backup '{args[1]}' deleted successfully.");
                    else
                        Console.WriteLine($"Failed to delete backup. Backup '{args[1]}' not found.");
                    return;
                }
                // Si l'argument est "list", lister tous les jobs
                else if (args[0].ToLower() == "list")
                {
                    Console.WriteLine("=== Backup Jobs ===");
                    int i = 1;
                    foreach (var job in _manager.Jobs)
                    {
                        Console.WriteLine($"{i++}. {job}");
                    }
                    return;
                }
                // Si l'argument est "logs", afficher les logs
                else if (args[0].ToLower() == "logs")
                {
                    _manager.ShowLogs();
                    return;
                }
                // Si l'argument est "help", afficher l'aide
                else if (args[0].ToLower() == "help")
                {
                    PrintHelp();
                    return;
                }
            }

            // Si aucun argument valide n'a été traité, utiliser l'interface interactive
            _language = _view.ChooseLanguage();
            while (true)
            {
                var opt = _view.DisplayMenu(_language);
                if (opt == "7") break; // Changed from 6 to 7
                HandleOption(opt);
            }
        }

        private void PrintHelp()
        {
            Console.WriteLine("EasySave Command Line Usage:");
            Console.WriteLine("  No arguments: Launch interactive interface");
            Console.WriteLine("  help: Show this help message");
            Console.WriteLine("  create <name> <source_path> <target_path> <type>: Create new backup");
            Console.WriteLine("  update <name> <source_path> <target_path> <type>: Update existing backup");
            Console.WriteLine("  delete <name>: Delete backup");
            Console.WriteLine("  list: List all backups");
            Console.WriteLine("  execute <indices>: Execute backups (e.g., 1-3 or 1;2;4)");
            Console.WriteLine("  <indices>: Execute backups (e.g., 1-3 or 1;2;4)");
            Console.WriteLine("  logs: Display logs");
        }

        private void HandleOption(string option)
        {
            switch (option)
            {
                case "1":
                    var jb = _view.AskNewBackupInfo(_language);
                    if (!_manager.AddJob(jb))
                        _view.ShowMessage("max_jobs", _language);
                    break;
                case "2":
                    _view.DisplayBackupList(_manager.Jobs);
                    if (_view.ConfirmSearch(_language))
                    {
                        var name = _view.AskBackupName(_language);
                        var b = _manager.GetJob(name);
                        if (b != null) _view.DisplayBackup(b);
                        else _view.ShowMessage("not_found", _language);
                    }
                    break;
                case "3":
                    {
                        var oldName = _view.AskBackupName(_language);
                        var nb = _view.AskNewBackupInfo(_language, oldName);
                        if (!_manager.UpdateJob(oldName, nb))
                            _view.ShowMessage("not_found", _language);
                    }
                    break;
                case "4":
                    {
                        var name = _view.AskBackupName(_language);
                        if (!_manager.RemoveJob(name))
                            _view.ShowMessage("not_found", _language);
                    }
                    break;
                case "5":
                    _manager.ShowLogs();
                    break;
                case "6": // Nouvelle option pour exécuter des sauvegardes
                    _view.DisplayBackupList(_manager.Jobs);
                    var args = _view.AskBackupIndices(_language);
                    if (args.Length > 0)
                    {
                        foreach (var arg in args)
                        {
                            var indices = ParseArgs(arg);
                            _manager.ExecuteJobsByIndices(indices);
                        }
                        _view.ShowMessage("exec_success", _language);
                    }
                    break;
                default:
                    _view.ShowMessage("invalid", _language);
                    break;
            }
        }

        private IEnumerable<int> ParseArgs(string arg)
        {
            var list = new List<int>();
            if (arg.Contains("-"))
            {
                var p = arg.Split('-');
                if (int.TryParse(p[0], out var a) && int.TryParse(p[1], out var b))
                    for (int i = a; i <= b; i++) list.Add(i);
            }
            else
            {
                foreach (var tok in arg.Split(';'))
                    if (int.TryParse(tok, out var x))
                        list.Add(x);
            }
            return list;
        }
    }
}