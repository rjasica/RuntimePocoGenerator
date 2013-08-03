## RuntimePocoGenerator

Emit poco objects from other or custom data at runtime.

![Icon(https://raw.github.com/rjasica/RuntimePocoGenerator/master/package_icon.png)


## Nuget 

Nuget package http://nuget.org/packages/RJ.RuntimePocoGenerator/

To Install from the Nuget Package Manager Console 
  
## Sample code

    public class Point
    {
        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public int X { get; private set; }
        public int Y { get; private set; }
    }

    public class Rectangle
    {
        public Rectangle(Point begin, Point end)
        {
             this.Begin = begin;
             this.End = end;
        }

        public Point Begin { get; private set; }
        public Point End { get; private set; }
    }
    
    var generator = new Generator()
    var result = generator.GenerateType(typeof(Rectangle));

    dynamic instance = Activator.CreateInstance(result.Type);

## What gets generaed

    public class Point
    {
        private int x;
        private int y;

        public Point()
        {
        }

        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public int X
        {
            get { return x; }
            set { this.x = value; }
        }

        public int Y
        {
            get { return y; }
            set { this.y = value; }
        }
    }

## Generating custom object

    var pointTypeDescription = new TypeDescription("Point", new List<IPropertyDescription>()
            {
                new PropertyDescription("X", typeof(int)),
                new PropertyDescription("Y", typeof(int))
            });
            
    var generator = new Generator()
    var result = generator.GenerateType(pointTypeDescription);

    dynamic instance = Activator.CreateInstance(result.Type);

## Icon

Icon courtesy of [The Noun Project](http://thenounproject.com)
