using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using ImageComparer.Models;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ImageComparer.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        /* コマンド、プロパティの定義にはそれぞれ 
         * 
         *  lvcom   : ViewModelCommand
         *  lvcomn  : ViewModelCommand(CanExecute無)
         *  llcom   : ListenerCommand(パラメータ有のコマンド)
         *  llcomn  : ListenerCommand(パラメータ有のコマンド・CanExecute無)
         *  lprop   : 変更通知プロパティ(.NET4.5ではlpropn)
         *  
         * を使用してください。
         * 
         * Modelが十分にリッチであるならコマンドにこだわる必要はありません。
         * View側のコードビハインドを使用しないMVVMパターンの実装を行う場合でも、ViewModelにメソッドを定義し、
         * LivetCallMethodActionなどから直接メソッドを呼び出してください。
         * 
         * ViewModelのコマンドを呼び出せるLivetのすべてのビヘイビア・トリガー・アクションは
         * 同様に直接ViewModelのメソッドを呼び出し可能です。
         */

        /* ViewModelからViewを操作したい場合は、View側のコードビハインド無で処理を行いたい場合は
         * Messengerプロパティからメッセージ(各種InteractionMessage)を発信する事を検討してください。
         */

        /* Modelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedEventListenerや
         * CollectionChangedEventListenerを使うと便利です。各種ListenerはViewModelに定義されている
         * CompositeDisposableプロパティ(LivetCompositeDisposable型)に格納しておく事でイベント解放を容易に行えます。
         * 
         * ReactiveExtensionsなどを併用する場合は、ReactiveExtensionsのCompositeDisposableを
         * ViewModelのCompositeDisposableプロパティに格納しておくのを推奨します。
         * 
         * LivetのWindowテンプレートではViewのウィンドウが閉じる際にDataContextDisposeActionが動作するようになっており、
         * ViewModelのDisposeが呼ばれCompositeDisposableプロパティに格納されたすべてのIDisposable型のインスタンスが解放されます。
         * 
         * ViewModelを使いまわしたい時などは、ViewからDataContextDisposeActionを取り除くか、発動のタイミングをずらす事で対応可能です。
         */

        /* UIDispatcherを操作する場合は、DispatcherHelperのメソッドを操作してください。
         * UIDispatcher自体はApp.xaml.csでインスタンスを確保してあります。
         * 
         * LivetのViewModelではプロパティ変更通知(RaisePropertyChanged)やDispatcherCollectionを使ったコレクション変更通知は
         * 自動的にUIDispatcher上での通知に変換されます。変更通知に際してUIDispatcherを操作する必要はありません。
         */

        public MainWindowViewModel()
        {
            this.CompareResultList = new ObservableCollection<ResultItem>();
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            { 
                CreateTestData();
            }
        }

        private void CreateTestData()
        {
            this.BeforePath = "あいうえお";
            this.AfterPath = "かきくけこ";
            /*
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            this.BeforeImage = image;

            image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            this.AfterImage = image;
            */
            this.CompareResultList.Add(new ResultItem() { AfterFilePath = "aaa1", BeforeFilePath = "bbb1" });
            this.CompareResultList.Add(new ResultItem() { AfterFilePath = "aaa2", BeforeFilePath = "bbb2" });
        }

        public void Initialize() { }

        public string BeforePath { get; set; }

        public string AfterPath { get; set; }

        private BitmapImage beforeImage;
        public BitmapImage BeforeImage
        {
            get { return this.beforeImage; }
            set
            {
                if (beforeImage == value) return;
                this.beforeImage = value;
                this.RaisePropertyChanged(() => this.BeforeImage);
            }
        }

        private BitmapImage afterImage;
        public BitmapImage AfterImage
        {
            get { return this.afterImage; }
            set
            {
                if (afterImage == value) return;
                this.afterImage = value;
                this.RaisePropertyChanged(() => this.AfterImage);
            }
        }

        private ResultItem compareResultListSelectedItem;
        public ResultItem CompareResultListSelectedItem
        {
            get { return this.compareResultListSelectedItem; }
            set
            {
                if (this.compareResultListSelectedItem == value)
                    return;
                this.compareResultListSelectedItem = value;
                this.RaisePropertyChanged(() => this.CompareResultListSelectedItem);
                if (this.compareResultListSelectedItem == null)
                {
                    return;
                }
                if (this.compareResultListSelectedItem.BeforeFilePath.Length > 0)
                {
                    var image1 = new BitmapImage();
                    image1.BeginInit();
                    image1.UriSource = new Uri(this.compareResultListSelectedItem.BeforeFilePath);
                    image1.CacheOption = BitmapCacheOption.OnLoad;
                    image1.EndInit();
                    this.BeforeImage = image1;
                }
                else { this.BeforeImage = null; }

                if (this.compareResultListSelectedItem.AfterFilePath.Length > 0)
                {
                    var image2 = new BitmapImage();
                    image2.BeginInit();
                    image2.UriSource = new Uri(this.compareResultListSelectedItem.AfterFilePath);
                    image2.CacheOption = BitmapCacheOption.OnLoad;
                    image2.EndInit();
                    this.AfterImage = image2;
                }
                else { this.AfterImage = null; }
            }
        }


        private ViewModelCommand sendDiffImgCommand;
        public ViewModelCommand SendDiffImgCommand
        {
            get
            {
                if (this.sendDiffImgCommand == null)
                {
                    this.sendDiffImgCommand = new ViewModelCommand(this.sendDiffImg);
                }
                return this.sendDiffImgCommand;
            }
        }

        private void sendDiffImg()
        {
            var diffImg = @"C:\Program Files\TheHive\DiffImg (x64)\diffimg.exe";
            if (!File.Exists(diffImg)) return;

            var arg = "\"" +  this.CompareResultListSelectedItem.BeforeFilePath
                + "\" \""
                + this.CompareResultListSelectedItem.AfterFilePath
                + "\"";
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(diffImg, arg);
            p.Start();
        }

        private ViewModelCommand compareCommand;
        public ViewModelCommand CompareCommand
        {
            get
            {
                if (this.compareCommand == null)
                {
                    this.compareCommand = new ViewModelCommand(this.compare);
                }
                return this.compareCommand;
            }
        }
        private void compare()
        {
            if (this.AfterPath.Length == 0 || this.BeforePath.Length == 0)
                return;
            this.CompareResultList.Clear();

            foreach (var afterFilePath in Directory.EnumerateFiles(this.AfterPath, "*.png", SearchOption.AllDirectories))
            {
                var beforeFilePath = afterFilePath.Replace(this.AfterPath, this.BeforePath);
                if (File.Exists(beforeFilePath))
                {
                    if (Md5Util.getMd5Hash(afterFilePath)
                        == Md5Util.getMd5Hash(beforeFilePath))
                    {
                        continue;
                    }
                    this.CompareResultList.Add(new ResultItem() { AfterFilePath = afterFilePath, BeforeFilePath = beforeFilePath });
                }
                else
                {
                    this.CompareResultList.Add(new ResultItem() { AfterFilePath = afterFilePath, BeforeFilePath = "" });
                }
            }
        }

        public ObservableCollection<ResultItem> CompareResultList { get; set; }
    }
}

public class ResultItem : ViewModel
{
    public string BeforeFilePath { get; set; }
    public string AfterFilePath { get; set; }
}
