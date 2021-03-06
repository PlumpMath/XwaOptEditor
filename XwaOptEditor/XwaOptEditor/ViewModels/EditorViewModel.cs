﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using JeremyAnsel.Xwa.Opt;
using JeremyAnsel.Xwa.WpfOpt;
using XwaOptEditor.Messages;
using XwaOptEditor.Models;
using XwaOptEditor.Mvvm;
using XwaOptEditor.Services;

namespace XwaOptEditor.ViewModels
{
    class EditorViewModel : ObservableObject
    {
        private object clipboardObject;

        private OptModel optModel;

        private int modelVersion = 0;

        private bool modelShowSolid = true;

        private bool modelShowWireframe = false;

        private float modelDistance = 0.001f;

        private bool showHitzones = true;

        private bool showHardpoints = true;

        private bool showHardpointsText = false;

        private bool showEngineGlows = true;

        public EditorViewModel()
        {
            this.CurrentMeshes = new SelectableCollection<Mesh>();
            this.CurrentLods = new SelectableCollection<MeshLod>();
            this.CurrentFaceGroups = new SelectableCollection<FaceGroup>();

            this.CurrentMeshes.SelectedItemChanged += (sender, e) =>
            {
                if (this.CurrentMeshes.SelectedItem == null)
                {
                    this.CurrentLods.LoadItems(null);
                }
                else
                {
                    this.CurrentLods.LoadItems(this.CurrentMeshes.SelectedItem.Lods);
                }
            };

            this.CurrentLods.SelectedItemChanged += (sender, e) =>
            {
                if (this.CurrentLods.SelectedItem == null)
                {
                    this.CurrentFaceGroups.LoadItems(null);
                }
                else
                {
                    this.CurrentFaceGroups.LoadItems(this.CurrentLods.SelectedItem.FaceGroups);
                }
            };

            this.OptModel = new OptModel();

            this.NewMeshCommand = new DelegateCommand(this.ExecuteNewMeshCommand);
            this.DeleteMeshesCommand = new DelegateCommandOfList<Mesh>(this.ExecuteDeleteMeshesCommand);
            this.UpMeshesCommand = new DelegateCommandOfList<Mesh>(this.ExecuteUpMeshesCommand);
            this.DownMeshesCommand = new DelegateCommandOfList<Mesh>(this.ExecuteDownMeshesCommand);
            this.SplitMeshesCommand = new DelegateCommandOfList<Mesh>(this.ExecuteSplitMeshesCommand);
            this.MergeMeshesCommand = new DelegateCommandOfList<Mesh>(this.ExecuteMergeMeshesCommand);
            this.MoveMeshesCommand = new DelegateCommandOfList<Mesh>(this.ExecuteMoveMeshesCommand);
            this.DuplicateMeshesCommand = new DelegateCommandOfList<Mesh>(this.ExecuteDuplicateMeshesCommand);
            this.ComputeHitzonesCommand = new DelegateCommand(this.ExecuteComputeHitzonesCommand);

            this.NewLodCommand = new DelegateCommand(this.ExecuteNewLodCommand);
            this.DeleteLodsCommand = new DelegateCommandOfList<MeshLod>(this.ExecuteDeleteLodsCommand);
            this.UpLodsCommand = new DelegateCommandOfList<MeshLod>(this.ExecuteUpLodsCommand);
            this.DownLodsCommand = new DelegateCommandOfList<MeshLod>(this.ExecuteDownLodsCommand);
            this.SortLodsCommand = new DelegateCommand(this.ExecuteSortLodsCommand);
            this.SplitLodsCommand = new DelegateCommandOfList<MeshLod>(this.ExecuteSplitLodsCommand);
            this.MergeLodsCommand = new DelegateCommandOfList<MeshLod>(this.ExecuteMergeLodsCommand);

            this.ComputeHitzoneCommand = new DelegateCommand(this.ExecuteComputeHitzoneCommand);

            this.NewHardpointCommand = new DelegateCommand(this.ExecuteNewHardpointCommand);
            this.DeleteHardpointsCommand = new DelegateCommandOfList<Hardpoint>(this.ExecuteDeleteHardpointsCommand);
            this.CutHardpointsCommand = new DelegateCommandOfList<Hardpoint>(this.ExecuteCutHardpointsCommand);
            this.CopyHardpointsCommand = new DelegateCommandOfList<Hardpoint>(this.ExecuteCopyHardpointsCommand);
            this.PasteHardpointsCommand = new DelegateCommand(this.ExecutePasteHardpointsCommand);

            this.NewEngineGlowCommand = new DelegateCommand(this.ExecuteNewEngineGlowCommand);
            this.DeleteEngineGlowsCommand = new DelegateCommandOfList<EngineGlow>(this.ExecuteDeleteEngineGlowsCommand);
            this.CutEngineGlowsCommand = new DelegateCommandOfList<EngineGlow>(this.ExecuteCutEngineGlowsCommand);
            this.CopyEngineGlowsCommand = new DelegateCommandOfList<EngineGlow>(this.ExecuteCopyEngineGlowsCommand);
            this.PasteEngineGlowsCommand = new DelegateCommand(this.ExecutePasteEngineGlowsCommand);

            this.AddTextureNameCommand = new DelegateCommand(this.ExecuteAddTextureNameCommand);
            this.BrowseTextureNameCommand = new DelegateCommand(this.ExecuteBrowseTextureNameCommand);
            this.DeleteTextureNamesCommand = new DelegateCommandOfList<string>(this.ExecuteDeleteTextureNamesCommand);

            this.SelectMeshCommand = new DelegateCommandOf<Tuple<MeshLodFace, Point3D>>(this.ExecuteSelectMeshCommand);
            this.AddMeshToSelectionCommand = new DelegateCommandOf<Tuple<MeshLodFace, Point3D>>(this.ExecuteAddMeshToSelectionCommand);
            this.AddHardpointCommand = new DelegateCommandOf<Tuple<MeshLodFace, Point3D>>(this.ExecuteAddHardpointCommand);
            this.AddEngineGlowCommand = new DelegateCommandOf<Tuple<MeshLodFace, Point3D>>(this.ExecuteAddEngineGlowCommand);
            this.CopyPointCommand = new DelegateCommandOf<Tuple<MeshLodFace, Point3D>>(this.ExecuteCopyPointCommand);

            Func<object, object> listBoxSelectedItemsSelector = sender => ((System.Windows.Controls.ListBox)sender).SelectedItems;

            this.HardpointsCutCopyCommandBindings = new CommandBindingCollection();
            this.HardpointsCutCopyCommandBindings.Add(ApplicationCommands.Cut, this.CutHardpointsCommand, listBoxSelectedItemsSelector);
            this.HardpointsCutCopyCommandBindings.Add(ApplicationCommands.Copy, this.CopyHardpointsCommand, listBoxSelectedItemsSelector);
            this.HardpointsPasteCommandBindings = new CommandBindingCollection();
            this.HardpointsPasteCommandBindings.Add(ApplicationCommands.Paste, this.PasteHardpointsCommand);

            this.EngineGlowsCutCopyCommandBindings = new CommandBindingCollection();
            this.EngineGlowsCutCopyCommandBindings.Add(ApplicationCommands.Cut, this.CutEngineGlowsCommand, listBoxSelectedItemsSelector);
            this.EngineGlowsCutCopyCommandBindings.Add(ApplicationCommands.Copy, this.CopyEngineGlowsCommand, listBoxSelectedItemsSelector);
            this.EngineGlowsPasteCommandBindings = new CommandBindingCollection();
            this.EngineGlowsPasteCommandBindings.Add(ApplicationCommands.Paste, this.PasteEngineGlowsCommand);
        }

        public ICommand NewMeshCommand { get; private set; }

        public ICommand DeleteMeshesCommand { get; private set; }

        public ICommand UpMeshesCommand { get; private set; }

        public ICommand DownMeshesCommand { get; private set; }

        public ICommand SplitMeshesCommand { get; private set; }

        public ICommand MergeMeshesCommand { get; private set; }

        public ICommand MoveMeshesCommand { get; private set; }

        public ICommand DuplicateMeshesCommand { get; private set; }

        public ICommand ComputeHitzonesCommand { get; private set; }

        public ICommand NewLodCommand { get; private set; }

        public ICommand DeleteLodsCommand { get; private set; }

        public ICommand UpLodsCommand { get; private set; }

        public ICommand DownLodsCommand { get; private set; }

        public ICommand SortLodsCommand { get; private set; }

        public ICommand SplitLodsCommand { get; private set; }

        public ICommand MergeLodsCommand { get; private set; }

        public ICommand ComputeHitzoneCommand { get; private set; }

        public ICommand NewHardpointCommand { get; private set; }

        public ICommand DeleteHardpointsCommand { get; private set; }

        public ICommand CutHardpointsCommand { get; private set; }

        public ICommand CopyHardpointsCommand { get; private set; }

        public ICommand PasteHardpointsCommand { get; private set; }

        public ICommand NewEngineGlowCommand { get; private set; }

        public ICommand DeleteEngineGlowsCommand { get; private set; }

        public ICommand CutEngineGlowsCommand { get; private set; }

        public ICommand CopyEngineGlowsCommand { get; private set; }

        public ICommand PasteEngineGlowsCommand { get; private set; }

        public ICommand AddTextureNameCommand { get; private set; }

        public ICommand BrowseTextureNameCommand { get; private set; }

        public ICommand DeleteTextureNamesCommand { get; private set; }

        public ICommand SelectMeshCommand { get; private set; }

        public ICommand AddMeshToSelectionCommand { get; private set; }

        public ICommand AddHardpointCommand { get; private set; }

        public ICommand AddEngineGlowCommand { get; private set; }

        public ICommand CopyPointCommand { get; private set; }

        public CommandBindingCollection HardpointsCutCopyCommandBindings { get; private set; }

        public CommandBindingCollection HardpointsPasteCommandBindings { get; private set; }

        public CommandBindingCollection EngineGlowsCutCopyCommandBindings { get; private set; }

        public CommandBindingCollection EngineGlowsPasteCommandBindings { get; private set; }

        public SelectableCollection<Mesh> CurrentMeshes { get; private set; }

        public SelectableCollection<MeshLod> CurrentLods { get; private set; }

        public SelectableCollection<FaceGroup> CurrentFaceGroups { get; private set; }

        public OptModel OptModel
        {
            get
            {
                return this.optModel;
            }

            set
            {
                if (this.optModel != value)
                {
                    if (this.optModel != null)
                    {
                        this.optModel.PropertyChanged -= this.OnOptModelPropertyChanged;
                    }

                    this.optModel = value;

                    if (this.optModel != null)
                    {
                        this.optModel.PropertyChanged += this.OnOptModelPropertyChanged;
                    }

                    this.RaisePropertyChangedEvent("OptModel");
                }
            }
        }

        public int ModelVersion
        {
            get
            {
                return this.modelVersion;
            }

            set
            {
                int max = this.OptModel.File == null ? 0 : this.OptModel.File.MaxTextureVersion;

                if (value < 0)
                {
                    value = 0;
                }
                else if (value > max)
                {
                    value = max;
                }

                if (this.modelVersion != value)
                {
                    this.modelVersion = value;
                    this.RaisePropertyChangedEvent("ModelVersion");
                }
            }
        }

        public bool ModelShowSolid
        {
            get
            {
                return this.modelShowSolid;
            }

            set
            {
                if (this.modelShowSolid != value)
                {
                    this.modelShowSolid = value;
                    this.RaisePropertyChangedEvent("ModelShowSolid");
                }
            }
        }

        public bool ModelShowWireframe
        {
            get
            {
                return this.modelShowWireframe;
            }

            set
            {
                if (this.modelShowWireframe != value)
                {
                    this.modelShowWireframe = value;
                    this.RaisePropertyChangedEvent("ModelShowWireframe");
                }
            }
        }

        public float ModelDistance
        {
            get
            {
                return this.modelDistance;
            }

            set
            {
                if (this.modelDistance != value)
                {
                    this.modelDistance = value;
                    this.RaisePropertyChangedEvent("ModelDistance");
                }
            }
        }

        public bool ShowHitzones
        {
            get
            {
                return this.showHitzones;
            }

            set
            {
                if (this.showHitzones != value)
                {
                    this.showHitzones = value;
                    this.RaisePropertyChangedEvent("ShowHitzones");
                }
            }
        }

        public bool ShowHardpoints
        {
            get
            {
                return this.showHardpoints;
            }

            set
            {
                if (this.showHardpoints != value)
                {
                    this.showHardpoints = value;
                    this.RaisePropertyChangedEvent("ShowHardpoints");
                }
            }
        }

        public bool ShowHardpointsText
        {
            get
            {
                return this.showHardpointsText;
            }

            set
            {
                if (this.showHardpointsText != value)
                {
                    this.showHardpointsText = value;
                    this.RaisePropertyChangedEvent("ShowHardpointsText");
                }
            }
        }

        public bool ShowEngineGlows
        {
            get
            {
                return this.showEngineGlows;
            }

            set
            {
                if (this.showEngineGlows != value)
                {
                    this.showEngineGlows = value;
                    this.RaisePropertyChangedEvent("ShowEngineGlows");
                }
            }
        }

        public void UpdateModel(bool preserveView = false)
        {
            if (preserveView)
            {
                var items = this.CurrentMeshes.ToList();

                this.CurrentMeshes.Clear();
                items.ForEach(t => this.CurrentMeshes.Add(t));
            }
            else
            {
                this.OptModel.File = this.OptModel.File;
            }
        }

        private void OnOptModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "File")
            {
                if (this.OptModel.File == null)
                {
                    this.CurrentMeshes.LoadItems(null);
                }
                else
                {
                    this.CurrentMeshes.LoadItems(this.OptModel.File.Meshes);
                }
            }
        }

        private void ExecuteNewMeshCommand()
        {
            BusyIndicatorService.Run(dispatcher =>
            {
                dispatcher(() => this.CurrentMeshes.ClearSelection());

                var mesh = new Mesh();
                mesh.Lods.Add(new MeshLod());

                this.OptModel.File.Meshes.Add(mesh);

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
            });
        }

        private void ExecuteDeleteMeshesCommand(IList<Mesh> meshes)
        {
            BusyIndicatorService.Run(dispatcher =>
            {
                dispatcher(() => this.CurrentMeshes.ClearSelection());

                foreach (var mesh in meshes)
                {
                    this.OptModel.File.Meshes.Remove(mesh);
                }

                dispatcher(() => this.UpdateModel());
            });
        }

        private void ExecuteUpMeshesCommand(IList<Mesh> meshes)
        {
            BusyIndicatorService.Run(dispatcher =>
            {
                var opt = this.OptModel.File;

                dispatcher(() => this.CurrentMeshes.ClearSelection());

                var orderedMeshes = meshes
                    .Select(mesh => new
                    {
                        Mesh = mesh,
                        Index = opt.Meshes.IndexOf(mesh)
                    })
                    .OrderBy(t => t.Index)
                    .Select(t => t.Mesh)
                    .ToList();

                foreach (var mesh in orderedMeshes)
                {
                    int index = opt.Meshes.IndexOf(mesh);

                    if (index == 0)
                    {
                        continue;
                    }

                    opt.Meshes.RemoveAt(index);
                    opt.Meshes.Insert(index - 1, mesh);
                }

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(meshes));
            });
        }

        private void ExecuteDownMeshesCommand(IList<Mesh> meshes)
        {
            BusyIndicatorService.Run(dispatcher =>
            {
                var opt = this.OptModel.File;

                dispatcher(() => this.CurrentMeshes.ClearSelection());

                var orderedMeshes = meshes
                    .Select(mesh => new
                    {
                        Mesh = mesh,
                        Index = opt.Meshes.IndexOf(mesh)
                    })
                    .OrderByDescending(t => t.Index)
                    .Select(t => t.Mesh)
                    .ToList();

                foreach (var mesh in orderedMeshes)
                {
                    int index = opt.Meshes.IndexOf(mesh);

                    if (index == opt.Meshes.Count - 1)
                    {
                        continue;
                    }

                    opt.Meshes.RemoveAt(index);
                    opt.Meshes.Insert(index + 1, mesh);
                }

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(meshes));
            });
        }

        private void ExecuteSplitMeshesCommand(IList<Mesh> meshes)
        {
            BusyIndicatorService.Run(dispatcher =>
            {
                var opt = this.OptModel.File;

                dispatcher(() => this.CurrentMeshes.ClearSelection());

                foreach (var mesh in meshes)
                {
                    opt.SplitMesh(mesh);
                }

                dispatcher(() => this.UpdateModel());
            });
        }

        private void ExecuteMergeMeshesCommand(IList<Mesh> meshes)
        {
            BusyIndicatorService.Run(dispatcher =>
            {
                bool doMerge = true;

                if (meshes.Sum(t => t.Vertices.Count - 2) > 510)
                {
                    var result = Messenger.Instance.Notify(new MessageBoxMessage(
                        "The merged mesh may contain more than 512 vertices.\nDo you want to continue?",
                        "Merge Meshes",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning));

                    doMerge = result.Result == MessageBoxResult.Yes;
                }

                if (doMerge)
                {
                    dispatcher(() => this.CurrentMeshes.ClearSelection());

                    var merge = this.OptModel.File.MergeMeshes(meshes);

                    if (merge != null)
                    {
                        merge.MergeLods(merge.Lods
                            .OrderBy(t => t.Distance)
                            .TakeWhile(t => t.Distance == merge.Lods[0].Distance)
                            .ToList());
                    }

                    dispatcher(() => this.UpdateModel());
                    dispatcher(() => this.CurrentMeshes.SetSelection(merge));
                }
            });
        }

        private void ExecuteMoveMeshesCommand(IList<Mesh> meshes)
        {
            BusyIndicatorService.Run(dispatcher =>
            {
                var message = Messenger.Instance.Notify(new MoveFactorMessage());

                if (!message.Changed)
                {
                    return;
                }

                BusyIndicatorService.Notify("Moving ...");

                foreach (var mesh in meshes)
                {
                    mesh.Move(message.MoveX, message.MoveY, message.MoveZ);
                }

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(meshes));
            });
        }

        private void ExecuteDuplicateMeshesCommand(IList<Mesh> meshes)
        {
            BusyIndicatorService.Run(dispatcher =>
            {
                BusyIndicatorService.Notify("Duplicating ...");

                foreach (var mesh in meshes)
                {
                    this.OptModel.File.Meshes.Add(mesh.Duplicate());
                }

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(meshes));
            });
        }

        private void ExecuteComputeHitzonesCommand()
        {
            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = this.CurrentMeshes.SelectedItem;

                this.OptModel.File.ComputeHitzones();

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
            });
        }

        private void ExecuteNewLodCommand()
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = this.CurrentMeshes.SelectedItem;

                var lod = new MeshLod();
                mesh.Lods.Add(lod);

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
                dispatcher(() => this.CurrentLods.SetSelection(lod));
            });
        }

        private void ExecuteDeleteLodsCommand(IList<MeshLod> lods)
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = this.CurrentMeshes.SelectedItem;

                dispatcher(() => this.CurrentLods.ClearSelection());

                foreach (var lod in lods)
                {
                    mesh.Lods.Remove(lod);
                }

                mesh.CompactBuffers();

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
            });
        }

        private void ExecuteUpLodsCommand(IList<MeshLod> lods)
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = this.CurrentMeshes.SelectedItem;

                dispatcher(() => this.CurrentLods.ClearSelection());

                var orderedLods = lods
                    .Select(lod => new
                    {
                        Lod = lod,
                        Index = mesh.Lods.IndexOf(lod)
                    })
                    .OrderBy(t => t.Index)
                    .Select(t => t.Lod)
                    .ToList();

                foreach (var lod in orderedLods)
                {
                    int index = mesh.Lods.IndexOf(lod);

                    if (index == 0)
                    {
                        continue;
                    }

                    mesh.Lods.RemoveAt(index);
                    mesh.Lods.Insert(index - 1, lod);
                }

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
                dispatcher(() => this.CurrentLods.SetSelection(lods));
            });
        }

        private void ExecuteDownLodsCommand(IList<MeshLod> lods)
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = this.CurrentMeshes.SelectedItem;

                dispatcher(() => this.CurrentLods.ClearSelection());

                var orderedLods = lods
                    .Select(lod => new
                    {
                        Lod = lod,
                        Index = mesh.Lods.IndexOf(lod)
                    })
                    .OrderByDescending(t => t.Index)
                    .Select(t => t.Lod)
                    .ToList();

                foreach (var lod in orderedLods)
                {
                    int index = mesh.Lods.IndexOf(lod);

                    if (index == mesh.Lods.Count - 1)
                    {
                        continue;
                    }

                    mesh.Lods.RemoveAt(index);
                    mesh.Lods.Insert(index + 1, lod);
                }

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
                dispatcher(() => this.CurrentLods.SetSelection(lods));
            });
        }

        private void ExecuteSortLodsCommand()
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = this.CurrentMeshes.SelectedItem;

                dispatcher(() => this.CurrentLods.ClearSelection());

                mesh.SortLods();

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
            });
        }

        private void ExecuteSplitLodsCommand(IList<MeshLod> lods)
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = this.CurrentMeshes.SelectedItem;

                dispatcher(() => this.CurrentLods.ClearSelection());

                foreach (var lod in lods)
                {
                    mesh.SplitLod(lod);
                }

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
            });
        }

        private void ExecuteMergeLodsCommand(IList<MeshLod> lods)
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = this.CurrentMeshes.SelectedItem;

                dispatcher(() => this.CurrentLods.ClearSelection());

                var mergedLod = mesh.MergeLods(lods);

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
                dispatcher(() => this.CurrentLods.SetSelection(mergedLod));
            });
        }

        private void ExecuteComputeHitzoneCommand()
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = this.CurrentMeshes.SelectedItem;

                mesh.ComputeHitzone();

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
            });
        }

        private void ExecuteNewHardpointCommand()
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = this.CurrentMeshes.SelectedItem;

                mesh.Hardpoints.Add(new Hardpoint());

                dispatcher(() => this.UpdateModel(true));
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
            });
        }

        private void ExecuteDeleteHardpointsCommand(IList<Hardpoint> hardpoints)
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = this.CurrentMeshes.SelectedItem;

                foreach (var hardpoint in hardpoints)
                {
                    mesh.Hardpoints.Remove(hardpoint);
                }

                dispatcher(() => this.UpdateModel(true));
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
            });
        }

        private void ExecuteCutHardpointsCommand(IList<Hardpoint> hardpoints)
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            this.clipboardObject = new Tuple<Mesh, IList<Hardpoint>>(this.CurrentMeshes.SelectedItem, hardpoints);
        }

        private void ExecuteCopyHardpointsCommand(IList<Hardpoint> hardpoints)
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            this.clipboardObject = new Tuple<Mesh, IList<Hardpoint>>(null, hardpoints);
        }

        private void ExecutePasteHardpointsCommand()
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            var selected = this.clipboardObject as Tuple<Mesh, IList<Hardpoint>>;

            if (selected == null)
            {
                return;
            }

            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = this.CurrentMeshes.SelectedItem;

                if (selected.Item1 == null)
                {
                    foreach (var hardpoint in selected.Item2)
                    {
                        mesh.Hardpoints
                            .Add(new Hardpoint
                            {
                                HardpointType = hardpoint.HardpointType,
                                Position = hardpoint.Position
                            });
                    }
                }
                else
                {
                    this.clipboardObject = null;

                    foreach (var hardpoint in selected.Item2)
                    {
                        selected.Item1.Hardpoints.Remove(hardpoint);
                        mesh.Hardpoints.Add(hardpoint);
                    }
                }

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
            });
        }

        private void ExecuteNewEngineGlowCommand()
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = this.CurrentMeshes.SelectedItem;

                mesh.EngineGlows.Add(new EngineGlow
                    {
                        Format = new JeremyAnsel.Xwa.Opt.Vector(50, 50, 1)
                    });

                dispatcher(() => this.UpdateModel(true));
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
            });
        }

        private void ExecuteDeleteEngineGlowsCommand(IList<EngineGlow> engineGlows)
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = this.CurrentMeshes.SelectedItem;

                foreach (var engineGlow in engineGlows)
                {
                    mesh.EngineGlows.Remove(engineGlow);
                }

                dispatcher(() => this.UpdateModel(true));
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
            });
        }

        private void ExecuteCutEngineGlowsCommand(IList<EngineGlow> engineGlows)
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            this.clipboardObject = new Tuple<Mesh, IList<EngineGlow>>(this.CurrentMeshes.SelectedItem, engineGlows);
        }

        private void ExecuteCopyEngineGlowsCommand(IList<EngineGlow> engineGlows)
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            this.clipboardObject = new Tuple<Mesh, IList<EngineGlow>>(null, engineGlows);
        }

        private void ExecutePasteEngineGlowsCommand()
        {
            if (this.CurrentMeshes.SelectedItem == null)
            {
                return;
            }

            var selected = this.clipboardObject as Tuple<Mesh, IList<EngineGlow>>;

            if (selected == null)
            {
                return;
            }

            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = this.CurrentMeshes.SelectedItem;

                if (selected.Item1 == null)
                {
                    foreach (var engineGlow in selected.Item2)
                    {
                        mesh.EngineGlows
                            .Add(new EngineGlow
                            {
                                IsDisabled = engineGlow.IsDisabled,
                                Position = engineGlow.Position,
                                CoreColor = engineGlow.CoreColor,
                                OuterColor = engineGlow.OuterColor,
                                Format = engineGlow.Format,
                                Look = engineGlow.Look,
                                Up = engineGlow.Up,
                                Right = engineGlow.Right
                            });
                    }
                }
                else
                {
                    this.clipboardObject = null;

                    foreach (var engineGlow in selected.Item2)
                    {
                        selected.Item1.EngineGlows.Remove(engineGlow);
                        mesh.EngineGlows.Add(engineGlow);
                    }
                }

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
            });
        }

        private void ExecuteAddTextureNameCommand()
        {
            if (this.CurrentFaceGroups.SelectedItem == null)
            {
                return;
            }

            BusyIndicatorService.Run(dispatcher =>
            {
                string fileName = FileDialogService.GetOpenTextureFileName();

                if (fileName == null)
                {
                    return;
                }

                try
                {
                    var texture = Texture.FromFile(fileName);

                    var mesh = this.CurrentMeshes.SelectedItem;
                    var lod = this.CurrentLods.SelectedItem;
                    var faceGroup = this.CurrentFaceGroups.SelectedItem;

                    this.OptModel.File.Textures[texture.Name] = texture;
                    faceGroup.Textures.Add(texture.Name);

                    dispatcher(() => this.UpdateModel());
                    dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
                    dispatcher(() => this.CurrentLods.SetSelection(lod));
                    dispatcher(() => this.CurrentFaceGroups.SetSelection(faceGroup));
                }
                catch (Exception ex)
                {
                    Messenger.Instance.Notify(new MessageBoxMessage(fileName, ex));
                }
            });
        }

        private void ExecuteBrowseTextureNameCommand()
        {
            if (this.CurrentFaceGroups.SelectedItem == null)
            {
                return;
            }

            BusyIndicatorService.Run(dispatcher =>
            {
                Messenger.Instance.Notify(new BusyIndicatorMessage("Browsing texture..."));

                var message = Messenger.Instance.Notify(new TextureBrowserMessage(this.OptModel.File));

                if (string.IsNullOrEmpty(message.TextureName))
                {
                    return;
                }

                var mesh = this.CurrentMeshes.SelectedItem;
                var lod = this.CurrentLods.SelectedItem;
                var faceGroup = this.CurrentFaceGroups.SelectedItem;

                faceGroup.Textures.Add(message.TextureName);

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
                dispatcher(() => this.CurrentLods.SetSelection(lod));
                dispatcher(() => this.CurrentFaceGroups.SetSelection(faceGroup));
            });
        }

        private void ExecuteDeleteTextureNamesCommand(IList<string> textureNames)
        {
            if (this.CurrentFaceGroups.SelectedItem == null)
            {
                return;
            }

            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = this.CurrentMeshes.SelectedItem;
                var lod = this.CurrentLods.SelectedItem;
                var faceGroup = this.CurrentFaceGroups.SelectedItem;

                foreach (var textureName in textureNames)
                {
                    faceGroup.Textures.Remove(textureName);
                }

                dispatcher(() => this.UpdateModel());
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
                dispatcher(() => this.CurrentLods.SetSelection(lod));
                dispatcher(() => this.CurrentFaceGroups.SetSelection(faceGroup));
            });
        }

        private void ExecuteSelectMeshCommand(Tuple<MeshLodFace, Point3D> tag)
        {
            this.CurrentMeshes.SetSelection(tag.Item1.Mesh);
            this.CurrentLods.SetSelection(tag.Item1.Lod);
            this.CurrentFaceGroups.SetSelection(tag.Item1.Face);
        }

        private void ExecuteAddMeshToSelectionCommand(Tuple<MeshLodFace, Point3D> tag)
        {
            this.CurrentMeshes.AddToSelection(tag.Item1.Mesh);
        }

        private void ExecuteAddHardpointCommand(Tuple<MeshLodFace, Point3D> tag)
        {
            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = tag.Item1.Mesh;

                mesh.Hardpoints
                    .Add(new Hardpoint
                    {
                        Position = new JeremyAnsel.Xwa.Opt.Vector((float)-tag.Item2.Y, (float)-tag.Item2.X, (float)tag.Item2.Z)
                    });

                dispatcher(() => this.UpdateModel(true));
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
            });
        }

        private void ExecuteAddEngineGlowCommand(Tuple<MeshLodFace, Point3D> tag)
        {
            BusyIndicatorService.Run(dispatcher =>
            {
                var mesh = tag.Item1.Mesh;

                mesh.EngineGlows
                    .Add(new EngineGlow
                    {
                        Position = new JeremyAnsel.Xwa.Opt.Vector((float)-tag.Item2.Y, (float)-tag.Item2.X, (float)tag.Item2.Z),
                        Format = new JeremyAnsel.Xwa.Opt.Vector(50, 50, 1)
                    });

                dispatcher(() => this.UpdateModel(true));
                dispatcher(() => this.CurrentMeshes.SetSelection(mesh));
            });
        }

        private void ExecuteCopyPointCommand(Tuple<MeshLodFace, Point3D> tag)
        {
            float scale = JeremyAnsel.Xwa.Opt.OptFile.ScaleFactor;

            var point = tag.Item2;

            var vector = new JeremyAnsel.Xwa.Opt.Vector(
                (float)-point.Y * scale,
                (float)-point.X * scale,
                (float)point.Z * scale);

            Clipboard.SetText(vector.ToString());
        }
    }
}
