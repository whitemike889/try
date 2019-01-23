using System;
using Microsoft.ML;
using MovieRecommendationConsoleApp.DataStructures;
using MovieRecommendation.DataStructures;
using Microsoft.ML.Data;

namespace MovieRecommendation
{
    class Program
    {
        // Using the ml-latest-small.zip as dataset from https://grouplens.org/datasets/movielens/. 
        private static string ModelsLocation = @"../../../../MLModels";
        public static string DatasetsLocation = @"../../../../Data";
        private static string TrainingDataLocation = $"{DatasetsLocation}/recommendation-ratings-train.csv";
        private static string TestDataLocation = $"{DatasetsLocation}/recommendation-ratings-test.csv";
        private static string MoviesDataLocation = $"{DatasetsLocation}/movies.csv";
        private const float predictionuserId = 6;
        private const int predictionmovieId = 10;

        static void Main(string[] args)
        {
            var mlcontext = new MLContext();

            #region build_model
            var reader = mlcontext.Data.CreateTextReader(new TextLoader.Arguments()
            {
                Separator = ",",
                HasHeader = true,
                Column = new[]
                {
                    new TextLoader.Column("userId", DataKind.R4, 0),
                    new TextLoader.Column("movieId", DataKind.R4, 1),
                    new TextLoader.Column("Label", DataKind.R4, 2)
                }
            });

            var trainingDataView = reader.Read(TrainingDataLocation);

            var pipeline = mlcontext.Transforms.Conversion.MapValueToKey("userId", "userIdEncoded")
                           .Append(mlcontext.Transforms.Conversion.MapValueToKey("movieId", "movieIdEncoded"))
                           .Append(mlcontext.Recommendation().Trainers.MatrixFactorization("userIdEncoded", "movieIdEncoded", "Label",
                           advancedSettings: s => { s.NumIterations = 20; s.K = 100; }));

            #endregion

         
            Console.WriteLine("=============== Training the model ===============");

            #region build_model
            var model = pipeline.Fit(trainingDataView);
            #endregion


            Console.WriteLine("=============== Evaluating the model ===============");

            #region evaluate_model
            var testDataView = reader.Read(TestDataLocation);
            var prediction = model.Transform(testDataView);
            var metrics = mlcontext.Regression.Evaluate(prediction, label: "Label", score: "Score");
            Console.WriteLine("The model evaluation metrics rms:" + Math.Round(float.Parse(metrics.Rms.ToString()), 1));
            #endregion


            #region prediction
            var predictionengine = model.CreatePredictionEngine<MovieRating, MovieRatingPrediction>(mlcontext);
            /* Make a single movie rating prediction, the scores are for a particular user and will range from 1 - 5. 
               The higher the score the higher the likelyhood of a user liking a particular movie.
               You can recommend a movie to a user if say rating > 3.5.*/
            var movieratingprediction = predictionengine.Predict(
                new MovieRating()
                {
                    //Example rating prediction for userId = 6, movieId = 10 (GoldenEye)
                    userId = predictionuserId,
                    movieId = predictionmovieId
                }
            );

            var movieService = new Movie();
            Console.WriteLine("For userId:" + predictionuserId + " movie rating prediction (1 - 5 stars) for movie:" + movieService.Get(predictionmovieId).movieTitle + " is:" + Math.Round(movieratingprediction.Score, 1));
            #endregion
        }
    }
}
