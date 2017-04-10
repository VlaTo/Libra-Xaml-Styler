using System;
using System.Collections;

namespace LibraProgramming.Parsing.Xaml
{
    public partial class XamlAttributesCollection
    {
        private struct XamlAttributesArray
        {
            private object container;

            public int Count
            {
                get
                {
                    if (null == container)
                    {
                        return 0;
                    }

                    var array = container as ArrayList;

                    if (null != array)
                    {
                        return array.Count;
                    }

                    return 1;
                }
            }

            public object this[int index]
            {
                get
                {
                    if (null == container)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    var array = container as ArrayList;

                    if (null != array)
                    {
                        return array[index];
                    }

                    if (0 != index)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    return container;
                }
            }

            public void Add(object item)
            {
                if (null == container)
                {
                    if (null == item)
                    {
                        container = new ArrayList
                        {
                            null
                        };
                    }
                    else
                    {
                        container = item;
                    }

                    return;
                }

                var array = container as ArrayList;

                if (null == array)
                {
                    container = new ArrayList
                    {
                        container,
                        item
                    };
                }
                else
                {
                    array.Add(item);
                }
            }

            public void Insert(int index, object item)
            {
                if (null == container)
                {
                    if (0 != index)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }

                    Add(item);

                    return;
                }

                var array = container as ArrayList;

                if (null != array)
                {
                    array.Insert(index, item);
                    return;
                }

                if (0 == index)
                {
                    container = new ArrayList
                    {
                        item,
                        container
                    };
                }
                else if (1 == index)
                {
                    container = new ArrayList
                    {
                        container,
                        item
                    };
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
            }

            public void RemoveAt(int index)
            {
                if (null == container)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                var array = container as ArrayList;

                if (null != array)
                {
                    array.RemoveAt(index);
                    return;
                }

                if (0 != index)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                container = null;
            }

            public IEnumerator GetEnumerator()
            {
                if (null == container)
                {
                    return EmptyEnumerator.Instance;
                }

                var array = container as ArrayList;

                if (null != array)
                {
                    return array.GetEnumerator();
                }

                return new SingleObjectEnumerator(container);
            }

            private class SingleObjectEnumerator : IEnumerator
            {
                private const int BeforeItem = -1;
                private const int AtItem = 0;
                private const int AfterItem = 1;

                private readonly object current;
                private int position;

                public object Current
                {
                    get
                    {
                        if (AtItem != position)
                        {
                            throw new InvalidOperationException();
                        }

                        return current;
                    }
                }

                public SingleObjectEnumerator(object current)
                {
                    this.current = current;
                    position = BeforeItem;
                }

                public bool MoveNext()
                {
                    position = BeforeItem == position ? AtItem : AfterItem;
                    return AtItem == position;
                }

                public void Reset()
                {
                    position = BeforeItem;
                }
            }
        }
    }
}