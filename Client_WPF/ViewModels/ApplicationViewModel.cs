using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Web_Service.Models;
using Web_Service.Helpers;
using Client_WPF.Models;
using Client_WPF.Cache;
using Client_WPF.Helpers;

namespace Client_WPF.ViewModels
{
    public class ApplicationViewModel : INotifyPropertyChanged
    {
        public ApplicationViewModel()
        {
            db = new TransportNodesdbContext();
            dbSize = db.TransportNodes.Count();

            TransportNodes = new ObservableCollection<TransportNodeInfo>();

            Cache = new Caching<TransportNodeInfo>();
            CacheList = new ObservableCollection<TransportNodeInfo>();

            Favorites = JDeserializer<ObservableCollection<TransportNodeInfo>>.Deser(ReadWriter<ObservableCollection<TransportNodeInfo>>.Read())
                ?? new ObservableCollection<TransportNodeInfo>();
        }

        private readonly TransportNodesdbContext db;
        public int dbSize { get; private set; }
        public ObservableCollection<TransportNodeInfo> TransportNodes { get; set; }

        private TransportNodeInfo selectedTransportNode;
        public TransportNodeInfo SelectedTransportNode
        {
            get { return selectedTransportNode; }
            set
            {
                if (value != null)
                {
                    if (value.Id != 0)
                    {
                        var item = Cache.GetOrCreate(value.Id.ToString(), value);

                        if (!CacheList.Contains(item))
                            CacheList.Insert(0, item);
                    }
                    else
                    {
                        CacheList.Remove(CacheList.FirstOrDefault(n => n.Id == 0));
                        CacheList.Insert(0, value);
                    }

                }

                selectedTransportNode = value;
                OnPropertyChanged("");
            }
        }

        private Caching<TransportNodeInfo> Cache { get; set; }
        public ObservableCollection<TransportNodeInfo> CacheList { get; set; }

        public ObservableCollection<TransportNodeInfo> Favorites { get; set; }

        // команда подгрузки данных в список
        private RelayCommand downCommand;
        public RelayCommand DownCommand
        {
            get
            {
                return downCommand ??
                  (downCommand = new RelayCommand(obj =>
                  {
                      int sizeList = (int)obj;

                      if (sizeList == 0)
                          sizeList = 1;

                      if (sizeList + 10 >= dbSize)
                      {
                          for (int i = sizeList; i < dbSize; i++)
                          {
                              TransportNodes.Add(db.TransportNodes.FirstOrDefault(p => p.Id == i));
                          }
                      }
                      else
                      {
                          for (int i = sizeList; i < sizeList + 10; i++)
                          {
                              TransportNodes.Add(db.TransportNodes.FirstOrDefault(p => p.Id == i));
                          }
                      }
                  },
                 (obj) => TransportNodes.Count < dbSize));
            }
        }

        // команда добавления данных в избранное
        private RelayCommand addCommand;
        public RelayCommand AddCommand
        {
            get
            {
                return addCommand ??
                  (addCommand = new RelayCommand(obj =>
                  {
                      TransportNodeInfo elem = obj as TransportNodeInfo;

                      if (elem != null && elem.Id != 0)
                      {
                          ReadWriter<List<TransportNodeInfo>>.Write(new List<TransportNodeInfo>() { elem });
                      }
                      else if (elem != null)
                      {
                          System.Windows.MessageBox.Show("Выбранный вами элемент уже находится в избранном.",
                              "Предупреждение", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                      }

                  },
                  (obj) => CacheList.Count > 0));
            }
        }

        // команда удаления данных из кэша
        private RelayCommand removeCommand;
        public RelayCommand RemoveCommand
        {
            get
            {
                return removeCommand ??
                  (removeCommand = new RelayCommand(obj =>
                  {
                      TransportNodeInfo elem = obj as TransportNodeInfo;

                      if (elem != null && elem.Id != 0)
                      {
                          CacheList.Remove(elem);
                          Cache.Remove(elem.Id.ToString());
                      }
                      else if (elem != null)
                          CacheList.Remove(elem);

                  },
                 (obj) => CacheList.Count > 0));
            }
        }

        // команда обновления вкладки избранное
        private RelayCommand updateCommand;
        public RelayCommand UpdateCommand
        {
            get
            {
                return updateCommand ??
                  (updateCommand = new RelayCommand(obj =>
                  {
                      List<TransportNodeInfo> temp_list = JDeserializer<List<TransportNodeInfo>>.Deser(ReadWriter<List<TransportNodeInfo>>.Read());

                      if (temp_list != null)
                      {
                          if (Favorites.Count == 0)
                          {
                              foreach (var item in temp_list)
                              {
                                  Favorites.Insert(0, item);
                              }
                          }
                          else
                          {
                              bool found;
                              foreach (TransportNodeInfo itemT in temp_list)
                              {
                                  found = false;

                                  foreach (var itemD in Favorites)
                                  {
                                       if (itemD.TPUName == itemT.TPUName)
                                        found = true;
                                  }

                                  if (!found)
                                  {
                                      Favorites.Insert(0, itemT);
                                  }

                              }
                          }
                      }

                  }
                  ));
            }
        }

        // команда очистки избранного
        private RelayCommand clearCommand;
        public RelayCommand ClearCommand
        {
            get
            {
                return clearCommand ??
                  (clearCommand = new RelayCommand(obj =>
                  {
                      ReadWriter<object>.Write(null);
                      Favorites.Clear();

                  },
                  (obj) => Favorites.Count > 0));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}