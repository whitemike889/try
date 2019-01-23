# Clustering Iris Data

| ML.NET version | API type          | Status                        | App Type    | Data type | Scenario            | ML Task                   | Algorithms                  |
|----------------|-------------------|-------------------------------|-------------|-----------|---------------------|---------------------------|-----------------------------|
| v0.9           | Dynamic API | Up-to-date | Console app | .txt file | Clustering Iris flowers | Clustering | K-means++ |

In this introductory sample, you'll see how to use [ML.NET](https://www.microsoft.com/net/learn/apps/machine-learning-and-ai/ml-dotnet) to divide iris flowers into different groups that correspond to different types of iris. In the world of machine learning, this task is known as **clustering**.

## Problem
To demonstrate clustering API in action, we will use three types of iris flowers: setosa, versicolor, and virginica. All of them are stored in the same dataset. Even though the type of these flowers is known, we will not use it and run clustering algorithm only on flower parameters such as petal length, petal width, etc. The task is to group all flowers into three different clusters. We would expect the flowers of different types belong to different clusters.

The inputs of the model are following iris parameters:
* petal length
* petal width
* sepal length
* sepal width

## ML task - Clustering
The generalized problem of **clustering** is to group a set of objects in such a way that objects in the same group are more similar to each other than to those in other groups.

Some other examples of clustering:
* group news articles into topics: sports, politics, tech, etc.
* group customers by purchase preferences.
* divide a digital image into distinct regions for border detection or object recognition.

Clustering can look similar to multiclass classification, but the difference is that for clustering tasks we don't know the answers for the past data. So there is no "tutor"/"supervisor" that can tell if our algorithm's prediction was right or wrong. This type of ML task is called **unsupervised learning**.

## Solution
To solve this problem, first we will build and train an ML model. Then we will use trained model for predicting a cluster for iris flowers.

### 1. Build model
Building a model includes: uploading data (`iris-full.txt` with `TextLoader`), transforming the data so it can be used effectively by an ML algorithm (with `Concatenate`), and choosing a learning algorithm (`KMeans`). All of those steps are stored in `trainingPipeline`:
```csharp --region create_model --project ./IrisClustering/IrisClustering/IrisClustering.csproj --session "iris clustering"     ./IrisClustering/IrisClustering/Program.cs
```
### 2. Train model
Training the model is a process of running the chosen algorithm on the given data. To perform training you need to call the Fit() method.
```csharp --region train_model --project ./IrisClustering/IrisClustering/IrisClustering.csproj --session "iris clustering"     ./IrisClustering/IrisClustering/Program.cs
```
### 3. Consume model
After the model is build and trained, we can use the `Predict()` API to predict the cluster for an iris flower and calculate the distance from given flower parameters to each cluster (each centroid of a cluster).

```csharp --region execute_model --project ./IrisClustering/IrisClustering/IrisClustering.csproj --session "iris clustering"     ./IrisClustering/IrisClustering/Program.cs
```
