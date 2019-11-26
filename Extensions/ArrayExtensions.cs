using System;

static class ArrayExtensions
{
    static private readonly Random rand = new Random();

    public static T PickRandom<T>(this T[] array) => array[rand.Next(array.Length)];
}
