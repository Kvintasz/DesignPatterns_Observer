# Observer pattern
[Observer pattern](https://en.wikipedia.org/wiki/Observer_pattern) is one of the [Behavioral pattern](https://en.wikipedia.org/wiki/Behavioral_pattern). With this pattern we make one or more of our objects updated with state change of another object.

## When should we use this pattern?
Imagine that situation when you want to buy the latest edition of your favourite newspaper in the morning but you don't know when the newspaper arrives to the newsstand where you want to buy it.
There are three option:
1. You go to the newstand and waiting for the newspaper. It is not appropiate because you are busy and you don't have infinit time for it.
2. You visit the newsstand in every hour. This need lot of time and energy.
3. You just **subscribe for the "Newspaper arrived" event**. You can do your work/fun until you got a notification from the newsstand about the newspaper arrived. You go to the newsstand and buy your favourit newspaper.

When you want to implement the third solution you can use **Observer pattern**.

## What do you need to implement this pattern?
* Observable (subject, publisher) - e.g. newsstand
  1. A collection for the subscribers (e.g. List`<Reader>` Subscribers)
  2. A public method by which the observers/subscribers can subscribe for the event (public void Subscribe(Observer observer))
  3. A public method by which the observers can unsubscribe from the event (public void Unsubscribe(Observer observer))
  4. A private method for the observable to notify the subscribers about the state change (private void NotifySubscribers())
* Observer - e.g. you
  1. A public method by which the observable/publisher/subject updates the observer about the state change (public void Update(Observable observable)). This call is in the Notify method of the observable.

## Example implementation
If mentioned the newspaper example I will implement it.
At first I implement the Newsstand. In the constructor I deal with the random event to simulate the newspaper arrival.

  public class Newsstand()
  {
    public Newsstand()
    {
      Random r = new Random();
      Thread.Sleep(r.Next(200,10000));
      NotifySubscribers();
    }
  
    public void Subscribe(Reader reader)
    {
      if(Subscribers.Contains(reader)) return;
      
      Subscribers.Add(reader);
    }
    
    public void Unsubscribe(Reader reader)
    {
      if(Subscribers.Contains(reader))
      {
        Subscribers.Remove(reader);
      }
    }
    
    private List<Reader> Subscribers = new List<>(Reader);
    
    private void NotifySubscribers()
    {
      foreach(Reader reader in  Subscribers)
      {
        reader.Update(this);
      }
    }
  }

And now I implement the reader, that is you. You need an update method. That is all.

  public class Reader()
  {
    Update(Newsstand stand)
    {
      Console.WriteLine("Go to the newsstand because the favourit newspaper has arrived!");
    }
  }
