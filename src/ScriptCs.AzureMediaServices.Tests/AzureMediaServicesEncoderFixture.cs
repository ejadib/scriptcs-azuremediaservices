namespace ScriptCs.AzureMediaServices.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.QualityTools.Testing.Fakes;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.WindowsAzure.MediaServices.Client;
    using Microsoft.WindowsAzure.MediaServices.Client.Fakes;

    [TestClass]
    public class AzureMediaServicesEncoderFixture
    {
        [TestMethod]
        public async Task WhenStartIsCalledThenAJobIsSubmitted()
        {
            const string Configuration = "H264 Broadband 720p";
            const string OutputAssetName = "Output Asset";
            const string JobName = "My Job";
            IAsset inputAsset = new StubIAsset { NameGet = () => "Input Asset" };
            var mediaProcessor = new StubIMediaProcessor
                                     {
                                         NameGet = () => "Windows Azure Media Encoder",
                                         VersionGet = () => "2.0.0"
                                     };

            string jobName = null;
            IMediaProcessor taskMediaProcessor = null;
            string taskConfiguration = null;
            IAsset taskInputAsset = null;
            string taskOutputAssetName = null;
            bool jobSubmitted = false;
            bool getExecutionProgressTaskCalled = false;

            using (ShimsContext.Create())
            {
                var outputAsset = new StubIAsset();

                var task = new StubITask();

                var tasks = new ShimTaskCollection
                                {
                                    AddNewStringIMediaProcessorStringTaskOptions =
                                        (taskName, processor, configuration, taskOptions) =>
                                            {
                                                taskMediaProcessor = processor;
                                                taskConfiguration = configuration;

                                                task.NameGet = () => taskName;

                                                return task;
                                            }
                                };

                var inputAssets = new ShimInputAssetCollection<IAsset> { AddT0 = asset => taskInputAsset = asset };
                var outputAssets = new ShimOutputAssetCollection
                                       {
                                           AddNewStringAssetCreationOptions =
                                               (outputAssetName, assetCreationOptions) =>
                                                   {
                                                       taskOutputAssetName = outputAssetName;
                                                       outputAsset.NameGet =
                                                           () => outputAssetName;
                                                       return outputAsset;
                                                   }
                                       };

                task.InputAssetsGet = () => inputAssets;
                task.OutputAssetsGet = () => outputAssets;

                var job = new StubIJob
                              {
                                  TasksGet = () => tasks,
                                  SubmitAsync = () =>
                                      {
                                          jobSubmitted = true;
                                          return Task.Delay(0);
                                      },
                                  GetExecutionProgressTaskCancellationToken = token =>
                                      {
                                          getExecutionProgressTaskCalled = true;

                                          return Task.Delay(0);
                                      }
                              };

                var jobs = new List<IJob>();
                var collection = new ShimJobBaseCollection
                                     {
                                         CreateString = name =>
                                             {
                                                 jobName = name;
                                                 job.NameGet = () => name;
                                                 return job;
                                             }
                                     };

                collection.Bind(jobs.AsQueryable());

                var mediaProcessors = new List<IMediaProcessor> { mediaProcessor };
                var mediaProcessorsCollection = new ShimMediaProcessorBaseCollection();
                mediaProcessorsCollection.Bind(mediaProcessors.AsQueryable());

                var context = new ShimCloudMediaContext
                                  {
                                      JobsGet = () => collection,
                                      MediaProcessorsGet = () => mediaProcessorsCollection
                                  };

                Func<CloudMediaContext> createContext = () => context;

                var encoder = new AzureMediaServicesEncoder(
                    JobName, inputAsset, Configuration, OutputAssetName, createContext);

                await encoder.Start();
            }

            Assert.AreEqual(JobName, jobName);
            Assert.AreEqual(Configuration, taskConfiguration);
            Assert.AreEqual(mediaProcessor, taskMediaProcessor);
            Assert.AreEqual(inputAsset, taskInputAsset);
            Assert.AreEqual(OutputAssetName, taskOutputAssetName);
            Assert.IsTrue(jobSubmitted);
            Assert.IsTrue(getExecutionProgressTaskCalled);
        }

        [TestMethod]
        public async Task WhenStateChangedEventIsRaisedAndJobIsProcessingThenProcessingCallbackIsCalled()
        {
            const string Configuration = "H264 Broadband 720p";
            const string OutputAssetName = "Output Asset";
            const string JobName = "My Job";
            const string JobId = "My Id";

            IAsset inputAsset = new StubIAsset { NameGet = () => "Input Asset" };
            var mediaProcessor = new StubIMediaProcessor
            {
                NameGet = () => "Windows Azure Media Encoder",
                VersionGet = () => "2.0.0"
            };

            string providedJobId = null;
            bool processingCalled = false;

            using (ShimsContext.Create())
            {
                var outputAsset = new StubIAsset();

                var task = new StubITask();

                var tasks = new ShimTaskCollection
                {
                    AddNewStringIMediaProcessorStringTaskOptions =
                        (taskName, processor, configuration, taskOptions) =>
                        {
                            task.NameGet = () => taskName;

                            return task;
                        }
                };

                var inputAssets = new ShimInputAssetCollection<IAsset> { AddT0 = asset => { } };
                var outputAssets = new ShimOutputAssetCollection
                {
                    AddNewStringAssetCreationOptions =
                        (outputAssetName, assetCreationOptions) =>
                        {
                            outputAsset.NameGet =
                                () => outputAssetName;
                            return outputAsset;
                        }
                };

                task.InputAssetsGet = () => inputAssets;
                task.OutputAssetsGet = () => outputAssets;

                var job = new StubIJob
                {
                    TasksGet = () => tasks,
                    SubmitAsync = () => Task.Delay(0),
                    GetExecutionProgressTaskCancellationToken = token => Task.Delay(0)
                };

                var jobs = new List<IJob>();
                var collection = new ShimJobBaseCollection
                {
                    CreateString = name =>
                    {
                        job.NameGet = () => name;
                        job.IdGet = () => JobId;
                        return job;
                    }
                };

                collection.Bind(jobs.AsQueryable());

                var mediaProcessors = new List<IMediaProcessor> { mediaProcessor };
                var mediaProcessorsCollection = new ShimMediaProcessorBaseCollection();
                mediaProcessorsCollection.Bind(mediaProcessors.AsQueryable());

                var context = new ShimCloudMediaContext
                {
                    JobsGet = () => collection,
                    MediaProcessorsGet = () => mediaProcessorsCollection
                };

                Func<CloudMediaContext> createContext = () => context;

                var encoder = new AzureMediaServicesEncoder(
                    JobName, inputAsset, Configuration, OutputAssetName, createContext);

                Action<string> onProcessing = jobId =>
                {
                    processingCalled = true;
                    providedJobId = jobId;
                };

                encoder.On(processing: onProcessing);

                await encoder.Start();

                job.StateChangedEvent.Invoke(job, new StubJobStateChangedEventArgs(JobState.Queued, JobState.Processing));

                Assert.IsTrue(processingCalled);
                Assert.AreEqual(JobId, providedJobId);
            }
        }

        [TestMethod]
        public async Task WhenStateChangedEventIsRaisedAndJobIsFinishedThenFinishedCallbackIsCalled()
        {
            const string Configuration = "H264 Broadband 720p";
            const string OutputAssetName = "Output Asset";
            const string JobName = "My Job";
            const string JobId = "My Id";

            IAsset inputAsset = new StubIAsset { NameGet = () => "Input Asset" };
            var mediaProcessor = new StubIMediaProcessor
                                     {
                                         NameGet = () => "Windows Azure Media Encoder",
                                         VersionGet = () => "2.0.0"
                                     };

            string providedJobId = null;
            bool finishedCalled = false;

            using (ShimsContext.Create())
            {
                var outputAsset = new StubIAsset();

                var task = new StubITask();

                var tasks = new ShimTaskCollection
                                {
                                    AddNewStringIMediaProcessorStringTaskOptions =
                                        (taskName, processor, configuration, taskOptions) =>
                                            {
                                                task.NameGet = () => taskName;

                                                return task;
                                            }
                                };

                var inputAssets = new ShimInputAssetCollection<IAsset> { AddT0 = asset => { } };
                var outputAssets = new ShimOutputAssetCollection
                                       {
                                           AddNewStringAssetCreationOptions =
                                               (outputAssetName, assetCreationOptions) =>
                                                   {
                                                       outputAsset.NameGet =
                                                           () => outputAssetName;
                                                       return outputAsset;
                                                   }
                                       };

                task.InputAssetsGet = () => inputAssets;
                task.OutputAssetsGet = () => outputAssets;

                var job = new StubIJob
                              {
                                  TasksGet = () => tasks,
                                  SubmitAsync = () => Task.Delay(0),
                                  GetExecutionProgressTaskCancellationToken = token => Task.Delay(0)
                              };

                var jobs = new List<IJob>();
                var collection = new ShimJobBaseCollection
                                     {
                                         CreateString = name =>
                                             {
                                                 job.NameGet = () => name;
                                                 job.IdGet = () => JobId;
                                                 return job;
                                             }
                                     };

                collection.Bind(jobs.AsQueryable());

                var mediaProcessors = new List<IMediaProcessor> { mediaProcessor };
                var mediaProcessorsCollection = new ShimMediaProcessorBaseCollection();
                mediaProcessorsCollection.Bind(mediaProcessors.AsQueryable());

                var context = new ShimCloudMediaContext
                                  {
                                      JobsGet = () => collection,
                                      MediaProcessorsGet = () => mediaProcessorsCollection
                                  };

                Func<CloudMediaContext> createContext = () => context;

                var encoder = new AzureMediaServicesEncoder(
                    JobName, inputAsset, Configuration, OutputAssetName, createContext);
                
                Action<string> onFinished = jobId =>
                {
                    finishedCalled = true;
                    providedJobId = jobId;
                };

                encoder.On(finished: onFinished);

                await encoder.Start();

                job.StateChangedEvent.Invoke(job, new StubJobStateChangedEventArgs(JobState.Processing, JobState.Finished));

                Assert.IsTrue(finishedCalled);
                Assert.AreEqual(JobId, providedJobId);
            }
        }

        [TestMethod]
        public async Task WhenStateChangedEventIsRaisedAndJobIsProcessingThenErrorCallbackIsCalled()
        {
            const string Configuration = "H264 Broadband 720p";
            const string OutputAssetName = "Output Asset";
            const string JobName = "My Job";
            const string JobId = "My Id";

            const string TaskId = "My Task Id";
            const string TaskErrorCode = "My Task Error Code";
            const string TaskErrorMessage = "My Task Error Message";

            IAsset inputAsset = new StubIAsset { NameGet = () => "Input Asset" };
            var mediaProcessor = new StubIMediaProcessor
            {
                NameGet = () => "Windows Azure Media Encoder",
                VersionGet = () => "2.0.0"
            };

            string errorMesage = null;
            bool errorCalled = false;

            using (ShimsContext.Create())
            {
                var outputAsset = new StubIAsset();

                var task = new StubITask
                               {
                                   IdGet = () => TaskId,
                                   ErrorDetailsGet =
                                       () =>
                                       new ReadOnlyCollection<ErrorDetail>(
                                           new List<ErrorDetail>
                                               {
                                                   new StubErrorDetail
                                                       {
                                                           Code = TaskErrorCode,
                                                           Message = TaskErrorMessage,
                                                       }
                                               })
                               };

                var tasks = new ShimTaskCollection
                {
                    AddNewStringIMediaProcessorStringTaskOptions =
                        (taskName, processor, configuration, taskOptions) =>
                        {
                            task.NameGet = () => taskName;

                            return task;
                        }
                };

                tasks.Bind(new List<ITask> { task }.AsQueryable());

                var inputAssets = new ShimInputAssetCollection<IAsset> { AddT0 = asset => { } };
                var outputAssets = new ShimOutputAssetCollection
                {
                    AddNewStringAssetCreationOptions =
                        (outputAssetName, assetCreationOptions) =>
                        {
                            outputAsset.NameGet =
                                () => outputAssetName;
                            return outputAsset;
                        }
                };

                task.InputAssetsGet = () => inputAssets;
                task.OutputAssetsGet = () => outputAssets;

                var job = new StubIJob
                {
                    TasksGet = () => tasks,
                    SubmitAsync = () => Task.Delay(0),
                    GetExecutionProgressTaskCancellationToken = token => Task.Delay(0)
                };

                var jobs = new List<IJob>();
                var collection = new ShimJobBaseCollection
                {
                    CreateString = name =>
                    {
                        job.NameGet = () => name;
                        job.IdGet = () => JobId;
                        return job;
                    }
                };

                collection.Bind(jobs.AsQueryable());

                var mediaProcessors = new List<IMediaProcessor> { mediaProcessor };
                var mediaProcessorsCollection = new ShimMediaProcessorBaseCollection();
                mediaProcessorsCollection.Bind(mediaProcessors.AsQueryable());

                var context = new ShimCloudMediaContext
                {
                    JobsGet = () => collection,
                    MediaProcessorsGet = () => mediaProcessorsCollection
                };

                Func<CloudMediaContext> createContext = () => context;

                var encoder = new AzureMediaServicesEncoder(
                    JobName, inputAsset, Configuration, OutputAssetName, createContext);

                Action<string> onError = message =>
                {
                    errorCalled = true;
                    errorMesage = message;
                };

                encoder.On(error: onError);

                await encoder.Start();

                job.StateChangedEvent.Invoke(job, new StubJobStateChangedEventArgs(JobState.Processing, JobState.Error));

                Assert.IsTrue(errorCalled);
                StringAssert.Contains(errorMesage, TaskId);
                StringAssert.Contains(errorMesage, TaskErrorCode);
                StringAssert.Contains(errorMesage, TaskErrorMessage);
            }
        }
    }
}