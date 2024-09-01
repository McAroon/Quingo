namespace Quingo;

public static class Extensions
{
    public static void MatchListSize<T>(this List<T> list, int size, Func<T> createNewFunc)
    {
        if (list.Count < size)
        {
            list.AddRange(Enumerable.Repeat((object)default!, size - list.Count).Select(_ => createNewFunc()));
        }
        else if (list.Count > size)
        {
            list.RemoveRange(size, list.Count - size);
        }
    }
}
