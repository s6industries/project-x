from src.object import Object
from src.character import Enemy
from src.UI import UI
import pygame

class Create:

    def __init__(self, screen, player, list_obj, selectItems=None, selectedItems=None):
        self.screen = screen
        self.player = player
        self.list_obj = list_obj #list of objects for map

        # retun this list
        self.listofObjects = [self.player] # for other objects
        self.listEnemies = [] # for enemies
        self.selectors = [] # for selectors
        self.listUIs = [] # for UI
        self.practice = None

        self.selected = 0

        #audios / sfx for selecting items
        self.selectItems = selectItems
        self.seletedItems = selectedItems

        # created
        self.created = False

    def create(self):
        for obj in self.list_obj:
            object = Object(obj['rect'][0], obj['rect'][1], obj['rect'][2], obj['rect'][3], obj['type'], obj['name'])
            if obj['name'] in ['box_1', 'box_2', 'box_3']:
                object.loadChestBox(obj['items'], self.player, self.screen)
            if object._type == 'practice':
                self.practice = object
            if obj['type'] == 'navigation':
                self.player.MapObjects[obj['distination']] = object
            self.listofObjects.append(object)

    def destroy(self):
        self.listofObjects = [self.player]
        self.listEnemies = []
                    
    def draw(self):
        for obj in self.listofObjects:
            if obj != self.listofObjects[0]:
                obj.draw(self.screen)

    def Reset(self):
        self.listofObjects = [self.player]
        self.create()

    # UI - Pause menu
    def pauseGame(self):
         keys = pygame.key.get_just_pressed()

         if keys[pygame.K_ESCAPE]:
             self.selectItems.play()
             return True
         
    def openWeapons(self):
        keys = pygame.key.get_just_pressed()

        if keys[pygame.K_f]:
            self.player.chestBoxSfx.play()
            return True

    #  FOR USER INTERFACE
    def create_UI(self):
        for obj in self.list_obj:
            ui = UI(obj['rect'][0], obj['rect'][1], obj['rect'][2], obj['rect'][3], obj['name'], obj['type'])
            if ui._type == 'selector':
                ui.selected = obj['selected']
                self.selectors.append(ui)
            self.listUIs.append(ui)

    def destory_UI(self):
        self.selectors = []
        self.listUIs = []
        self.selected = 0

    def draw_ui(self):
        for uis in self.listUIs:
            uis.draw(self.screen)
        # self.Select()

    def Select(self): # select menu
        keys = pygame.key.get_just_pressed()

        if keys[pygame.K_TAB] or keys[pygame.K_DOWN]:
            self.selectItems.play()
            self.selected += 1
            if self.selected > (len(self.selectors) - 1):
                self.selected = 0

            for ui in self.selectors:
                ui.selected = 1
            self.selectors[self.selected].selected = 0

        elif keys[pygame.K_UP]:
            self.selectItems.play()
            self.selected -= 1
            if self.selected < 0:
                self.selected = (len(self.selectors) - 1)

            for ui in self.selectors:
                ui.selected = 1
            self.selectors[self.selected].selected = 0
        
        elif keys[pygame.K_SPACE]:
            self.seletedItems.play()
            return self.selected

    # for enemies
    def create_enemies(self):
        for enemy in self.list_obj:
            enmy = Enemy(enemy['rect'][0], enemy['rect'][1], enemy['rect'][2], enemy['rect'][3], enemy['name'])
            enmy.speed = float(enemy['speed'])
            enmy.defaultLife = int(enemy['default_life'])
            enmy.life = int(enemy['default_life'])
            self.listEnemies.append(enmy)

    def draw_enemy(self, objects):
        for enemy in self.listEnemies:
            if enemy.life <= 0:
                enemy.hit_effects(self.screen)
                if enemy.out:
                    self.listEnemies.remove(enemy)
            enemy.draw(self.screen, objects)
            enemy.follow(self.player) # follow the player
            enemy.Attack(self.player)

    def resetEnemies(self):
        self.listEnemies = []
        self.create_enemies()