import pygame
import random
from .character import Zombie
from .weapons import Items
import json
from . import timer, object
from . import weapons

class Game:

    def __init__(self, screen):
        self.screen = screen
        self.play = False # if the player already played
        self.location = None
        self.player = None
        self.collectables = None # player's collected
        
        self.colors = {
            "White": (255,255,255),
            "red": (232, 51, 35),
            "green": (35, 235, 88)
        }

        self.message_from_past = None
        self.font_one = pygame.font.SysFont('consolas', 15)
        self.transitionTimer = timer.Timer(30)
        self.music = None
        self.switch_music = True

        self.Enemies = []
        self.items = [] # from enemies

        self.tries = 3 # only 3 tries, if 0 then game over
        self.gameover = False

        # items data
        self.itemData = list()
        self.loadItemsData()

        self.num_enemies_nLvl = [6, 10, 15]
        self.nextLevel = False
        self.transition = True
        self.end_transition = False
        self.transDuration = 30

        # for boss fight transition
        self.objects = []

        # creadits text
        self.credits = None
        self.loadCredits()

    def init(self):
        self.switch_music = True

        self.Enemies = []
        self.items = [] # from enemies

        self.tries = 3 # only 3 tries, if 0 then game over
        self.gameover = False

        self.nextLevel = False
        self.transition = True
        self.end_transition = False
        self.transDuration = 30
        self.gameover_transition_count = 150 # count for game over transition
        self.endgame_message = f'I, {self.player.name.title()}, Back in Time!'

        self.reading_page = 0


    def Reset(self, createEnemy, map, creation_obj): 
        if self.player.life <= 0:
            if self.tries > 1:
                self.tries -= 1
                self.player.life = self.player.defaultLife
                self.player.mana = self.player.defaultMana
                self.player.loading = True
                self.player.nav = True
                self.player.loading_timer = self.player.loading_timer_max
                self.player.loading_nav(self.screen)

                if createEnemy:
                    self.Enemies = [] 
                    self.createEnemies() # create new enemies
                if creation_obj:
                    creation_obj.Reset()
                
                # reset the map
                map.x = 0
                map.y = 0
            else:
                self.gameover = True
                self.play = False
                self.music.switch = True
                self.music.toPlay = 1

    def Reset_bossFight(self, map, creation_obj, boss):
        if self.player.life <= 0:
            if self.tries > 1:
                self.tries -= 1
                self.player.life = self.player.defaultLife
                self.player.mana = self.player.defaultMana
                self.player.loading = True
                self.player.nav = True
                self.player.loading_timer = self.player.loading_timer_max
                self.player.loading_nav(self.screen)

                if creation_obj:
                    creation_obj.Reset()

                self.Enemies = []
                boss.Reset()
                boss.darkBalls = []

                map.x = 0
                map.y = 0
            else:
                self.gameover = True
                self.play = False
                self.music.switch = True
                self.music.toPlay = 1

    def CheckBossFight(self, boss):
        boss[self.player.level-1].toFocus = self.player
        if boss[self.player.level-1].life <= 0:
            boss[self.player.level-1].weapon[0].triggered = False
            boss[self.player.level-1].weapon[0].player = None
            if len(self.player.myWeapons) < 8:
                self.player.myWeapons.append(boss[self.player.level-1].weapon[0]) # fix this - out of range if the player's weapons is 8
                self.player.myWeapons.append(boss[self.player.level-1].potion[0])
                boss[self.player.level-1].potion[0].player = self.player # making the potion to player
            boss[self.player.level-1] = None
            self.player.can_return = True
            self.Enemies = []

            # add new level
            self.add_level() # add level to the player
            self.player.nav = True
            self.player.loading = True
            self.player.nav = True
            self.player.respawn = self.player.location
            self.player.location = 'map3'
            self.player.loading_timer = self.player.loading_timer_max
            self.player.score = 0 # reset the player's score

            # reset all player collected
            self.collectables.collectedItems = []
            self.collectables.collectedItemsCode = []

    def addEnemies_BossFight(self):
        if not len(self.Enemies):
            if not random.choice(range(0, 100)):
                self.createEnemies(-300, -300, -250, -250)

    def ResetMap(self, map, creation_obj):
        if self.player.nextMap and len(self.Enemies) < 1: # check this shit
            map.x = 0
            map.y = 0

            if creation_obj != None:
                creation_obj.Reset()

            # create enemies
            self.items = [] # clear items
            self.createEnemies()

            self.player.loading = True
            self.player.nav = True
            self.player.loading_timer = self.player.loading_timer_max
            
            self.player.rect.x = self.player.MapObjects[f'{self.player.respawn}_{self.player.location}'].rect.x
            self.player.rect.y = self.player.MapObjects[f'{self.player.respawn}_{self.player.location}'].rect.y
            
        self.player.nextMap = False

    def showMonitor_message(self, monitor, screen):
        keys = pygame.key.get_just_pressed()

        self.message_from_past = [
            f'New Message:\n\nDear {self.player.name.title()},\nThe world as we knew has fallen.\nZombies have conquired everything,\nand you are the last surviving \nhuman.\nBut there is still hope. You\nmust time travel to the past to \nstop the zombie virus from \nspreading.\n\nNext>>', 
            '1. The missing pieces of the time \nmachine are scattered around this \nlab. \nYou need to find them. \n2. There\'s a book \nsomewhere in the lab that contains\ninstructions on how to craft other \nessential parts of the machine.\n3. Be cautious, as zombies \nroam the area. \nYou\'ll need \nto find weapons to defend \nyourself.\n\nNext>>',
            'Good luck, the world is in your \nhands.\nBack in time to save the world.\n\n-------------------------\nSender:\nPresident Baldesoto\nPresident of the Philippines\n01/23/3024'
        ]
        text = self.font_one.render(self.message_from_past[self.reading_page], True, self.colors['green'])
        screen.blit(text, (monitor[0]+20, monitor[1]+20))

        if keys[pygame.K_RIGHT]:
            self.reading_page += 1

            if self.reading_page > len(self.message_from_past)-1:
                self.reading_page = 0
        elif keys[pygame.K_LEFT]:
            self.reading_page -= 1

            if self.reading_page < 0:
                self.reading_page = len(self.message_from_past)-1

    def createEnemies(self, xs=100, ys=100, xn=600, yn=800):
        # random 0 - 6 enemies
        num_enemies = random.choice(range(self.num_enemies_nLvl[self.player.level-1]))

        for _ in range(num_enemies):
            posx = random.choice(list(range(xs, xn)))
            posy = random.choice(list(range(ys, yn)))
            enemy = Zombie(posx, posy, 50, 50, 'zombie')
            enemy.speed = random.choice([2,3,4])
            enemy.defaultLife = random.choice([20, 30, 10, 20, 20, 20, 20])
            enemy.life = enemy.defaultLife
            enemy.items = self.generateItems() # give item
            self.Enemies.append(enemy)

    def generateItems(self):
        count = random.choice([0,0,0,0,0,1,1,1,1])
        items = []

        for _ in range(count):
            item = random.choice(self.itemData)
            item = Items(self.player, self.screen, (40,40), item)
            if item.checkItem():
                if self.findItems(item) and item not in items: # if item not in self.items and in item lists
                    items.append(item)

        return items # list of items

    def drawItems(self):
        for item in self.items:
            itemImage = pygame.transform.scale(item.icon, (20, 20))
            self.screen.blit(itemImage, (item.x, item.y))

    def loadItemsData(self):
        with open('Data/items.json') as file:
            Data = json.load(file)

            craftItems = Data['craftItems']
            item = list(Data['Items'])
            for i in item:
                if i not in craftItems:
                    self.itemData.append(i)

    def loadCredits(self):
        with open('Data/credits.json') as file:
            Data = json.load(file)

            contributers = Data['contributers']

            self.credits = f"{contributers[0]['name']}\n{contributers[0]['rule']}\n\n{contributers[1]['name']}\n{contributers[1]['rule']}\n\n{contributers[2]['name']}\n{contributers[2]['rule']}"

    def findItems(self, item):
        for i in self.items:
            if i.code == item.code:
                print('found')
                return False # return true if the item is already exist in self.items
            
        for weapon in self.player.myWeapons:
            if weapon.code == item.code:
                return False
            
        return True
    
    # add level to a player
    def add_level(self):
        if self.player.level <= 3:
            self.player.level += 1
            print('player level: ' + str(self.player.level))

    def boss_transition(self):
        if self.transition or self.end_transition:
            if self.transitionTimer.countDown >= 1:
                self.screen.fill((255,255,255))
                self.objects = [] # remove all the object
            else:
                self.screen.fill((0,0,0))
            
            # swicth the music
            if self.switch_music:
                self.music.switch = True
                self.music.toPlay = 2
                self.switch_music = False
            self.player.can_move = False

            # draw 2 objects for the transition

            for obj in self.objects:
                obj.draw(self.screen)

            # end the transition 
            if self.transitionTimer.coolDown(11):

                # switch music
                self.music.switch = True

                # reset the transition
                if self.transition:
                    self.transition = False
                    self.end_transition = True
                    self.music.toPlay = 3
                else:
                    self.transition = True
                    self.end_transition = False
                    self.music.toPlay = 0
                
                self.switch_music = True # transition music
                self.player.can_move = True

                self.objects = [ # for the transition
                    object.Object(302, 202, 96, 96, 'animated2', 'teleport_machine', True), # the teleportation machine
                    object.Object(325, 225, 50, 50, 'other', self.player.name) # the player
                ]
    
    def UpgradeShield(self):
        if self.player.score >= 60:
            if len(self.player.myWeapons) < 8:
                self.player.myWeapona.append(weapons.Shield(self.player, self.screen, (80, 80), 'protektor'))
                self.score = 0
        elif self.player.score >= 100:
            if len(self.player.myWeapons) < 8:
                self.player.myWeapons.append(weapons.Shield(self.player, self.screen, (80, 80), 'armored'))
                self.score = 0
    
    def endgameExit(self, objects=None):
        keys = pygame.key.get_just_pressed()

        if self.transitionTimer.countDown > 1 and self.transitionTimer.countDown <= 10:
            text = self.font_one.render(self.endgame_message, True, (0,0,0))
            self.screen.blit(text, ((700 - text.get_width()) / 2, (500 - text.get_height()) / 2, text.get_width(), text.get_height()))
        
        elif self.transitionTimer.countDown > 10:
            if objects:
                for obj in objects:
                    obj.draw(self.screen)

        if self.transitionTimer.coolDown(121):
            return True
        
        elif keys[pygame.K_SPACE]:
            self.music.switch = True
            self.music.toPlay = 1
            return True