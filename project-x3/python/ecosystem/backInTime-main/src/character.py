import pygame
from . import skills # Speed, Boomerang, shield, copy
from . import timer
import random

class Player(pygame.sprite.Sprite):

    def __init__(self, x, y, width, height, name=''):
        super().__init__()
        self.width = width
        self.height = height
        self.name = name

        # Fonts
        self.font = pygame.font.SysFont('consolas', 15)
        self.messageFont = pygame.font.SysFont('consolas', 15)

        # timers
        self.timer = timer.Timer(30) # fps = 30
        self.messageTimer = timer.Timer(30)

        # rect and surface
        self.rect = pygame.Rect((x, y), (self.width, self.height)) # for player rect

        # self.Init(x, y) # initialize all
        self.bossCount = 0 # number of boss

        # map objects
        self.MapObjects = {}

        # sound effects
        self.walkSfx = None
        self.chestBoxSfx = pygame.mixer.Sound('audio/openChest.mp3')
        self.sfx = []
        self.walkSfx_timer = 0
        self.hit_sfx = pygame.mixer.Sound('audio/hit.mp3')
        self.sfx_timer = 0
        self.pickItem_sfx = pygame.mixer.Sound('audio/pickItem.mp3')
        self.door = pygame.mixer.Sound('audio/door.mp3')

    def draw(self, screen, allObj):
        if self.walkSfx_timer > 0:
            self.walkSfx_timer -= 1

        # handle collision
        self.handleCollision(allObj)

        self.displayName = self.font.render(self.name.title(), True, (0,0,0)) # temporary 
        screen.blit(self.displayName, (self.rect.x, self.rect.y - (self.width / 2), self.displayName.get_width(), self.displayName.get_height()))

        # get key events
        keys = pygame.key.get_pressed()

        # left
        if keys[pygame.K_LEFT] or keys[pygame.K_a]:
            if self.can_move:
                self.left = True
                self.right = False
                self.up = False
                self.down = False

                self.move_x((self.speed * -1))

        # right
        elif keys[pygame.K_RIGHT] or keys[pygame.K_d]:
            if self.can_move:
                self.left = False
                self.right = True
                self.up = False
                self.down = False

                self.move_x(self.speed)

        # up
        elif keys[pygame.K_UP] or keys[pygame.K_w]:
            if self.can_move:
                self.left = False
                self.right = False
                self.up = True
                self.down = False

                self.move_y((self.speed * -1))

        elif keys[pygame.K_DOWN] or keys[pygame.K_s]:
            if self.can_move:
                self.left = False
                self.right = False
                self.up = False
                self.down = True

                self.move_y(self.speed)
        else: 
            self.walk = 0

        self.Facing(screen)
        self.masterWeapon()
        
        # player rect
        # pygame.draw.rect(screen, (255,255,10), self.rect, 1)

    def barLife(self, screen):
        # life bar
        pygame.draw.rect(screen, (0, 16, 41), (65, 20, 195, 20))
        pygame.draw.rect(screen, (7, 247, 227), (65, 20, (self.life / self.defaultLife) * 195, 20))
        
        # mana bar
        pygame.draw.rect(screen, (0, 16, 41), (73, 45, 173, 10))
        pygame.draw.rect(screen, (255, 120, 241), (73, 45, (self.mana / self.defaultMana) * 173, 10))

        # skill bar
        pygame.draw.rect(screen, (0, 16, 41), (68, 53, 173, 10))
        pygame.draw.rect(screen, (255, 84, 126), (68, 53, (self.skill_cooldown / self.skills.skill_cooldown) * 173, 10))
    
    def Facing(self, screen):
        # self.walk is the number of walk of character
        if (self.walk + 1) >= 9:
            self.walk = 0
            
        if self.left:
            screen.blit(self.c_left[self.walk//3], (self.rect.x, self.rect.y))
            self.walk += 1
        elif self.right:
            screen.blit(self.c_right[self.walk//3], (self.rect.x, self.rect.y))
            self.walk += 1
        elif self.up:
            screen.blit(self.c_up[self.walk//3], (self.rect.x, self.rect.y))
            self.walk += 1
        elif self.down:
            screen.blit(self.c_down[self.walk//3], (self.rect.x, self.rect.y))
            self.walk += 1
        else:
            if self.left:
                screen.blit(self.c_left[0], (self.rect.x, self.rect.y))
            elif self.right:
                screen.blit(self.c_right[0], (self.rect.x, self.rect.y))

    # load all image in list of images [right, left, and down]
    def loadImages(self):
        images = ['D_Walk_', 'S_Walk_', 'U_Walk_']

        for image in images:
            for count in range(3):
                img = f'characters/{self.name}/{image}{count}.png'
                img = pygame.image.load(img)
                img = pygame.transform.scale(img, (self.width, self.height))
                if image == 'D_Walk_':
                    self.c_down.append(img)
                elif image == 'U_Walk_':
                    self.c_up.append(img)
                elif image == 'S_Walk_':
                    self.c_right.append(img)

        self.flipImage()

    # flip all characters to right
    def flipImage(self):
        for character in self.c_right:
            self.c_left.append(pygame.transform.flip(character, True, False))
    
    # [direction] positive number going to right, negative going to left
    def move_x(self, direction):
        if self.walkSfx_timer == 0:
            self.walkSfx_timer = 25
            self.walkSfx.play()
        self.rect.x += direction

    # [direction] positive number going down, negative going up
    def move_y(self, direction):
        if self.walkSfx_timer == 0:
            self.walkSfx_timer = 25
            self.walkSfx.play()
        self.rect.y += direction

    def handleCollision(self, objects):
        for obj in objects:

            # handle facing of the player
            if self.left or self.right:
                if self.rect.y > obj.rect.y:
                    obj.front = True
                elif self.rect.y <= obj.rect.y:
                    obj.front = False

            # if the player collide with the objects
            if pygame.sprite.collide_rect(self, obj):
                # chest boxes and craftbox
                if obj._type in ['hidden2', 'other2']: # no y-sorting objects
                    if obj.name in ['crafting_table']:
                        self.openCraftBox() # open a crafting table

                    # other collisions
                    if self.left:
                        self.rect.left = obj.rect.right
                    elif self.right:
                        self.rect.right = obj.rect.left
                    elif self.up:
                        self.rect.top = obj.rect.bottom
                    elif self.down:
                        self.rect.bottom = obj.rect.top
                elif obj._type in ['other', 'hidden', 'animated', 'animated_once']: # with y-sorting objects
                    # check if item is chestbox
                    if obj.name in ['box_1', 'box_2', 'box_3'] and obj.loaded:
                        for item in obj.loaded:
                            self.pick(item, obj) # pick the item with space bar
                        if len(obj.loaded) < 1:
                            self.viewVaultBox = True
                    elif obj.name in ['vault_box']:
                        self.openChestBox()
                    elif obj.name == 'machine_2':
                        self.openCollectable()
                    elif obj.name == 'monitor_1':
                        self.showGuide()
                    elif obj.name == 'book_1':
                        self.openBook()

                    # handle facing and collision for object - with y-sorting
                    if self.left:
                        if self.rect.y <= obj.rect.y:
                            self.rect.left = obj.rect.right
                    elif self.right:
                        if self.rect.y <= obj.rect.y:
                            self.rect.right = obj.rect.left

                    elif self.up and obj.front:
                        if self.rect.top <= obj.rect.bottom - 40:
                            self.rect.top = obj.rect.bottom - 40
                    elif self.down and not(obj.front):
                        self.rect.bottom = obj.rect.top

                # example door or the time machine
                elif obj._type == 'navigation':
                    if obj.name == 'NextBattleMap':
                        self.nextMap = True
                    else:
                        self.checkLocation(obj)
    
    # handling navigating to other location
    def checkLocation(self, obj):
        
        # press spacebar to open door
        keys = pygame.key.get_just_pressed()

        if keys[pygame.K_SPACE]:
            self.loading = True

            if self.loading_timer <= 0:
                if obj.name == 'base':
                    self.nav = True
                    self.right, self.left, self.up, self.down, = False, True, False, False
                    self.respawn = self.location
                    self.location = obj.name

                elif obj.name == 'map2':
                    self.nav = True
                    self.right, self.left, self.up, self.down, = False, False, True, False
                    self.respawn = self.location
                    self.location = obj.name

                elif obj.name == 'map3':
                    if self.can_return:
                        self.nav = True
                        self.right, self.left, self.up, self.down, = False, False, True, False
                        self.respawn = self.location
                        self.location = obj.name

                elif obj.name == 'map4':
                    self.nav = True
                    self.right, self.left, self.up, self.down, = True, False, False, False
                    self.respawn = self.location
                    self.location = obj.name

                elif obj.name == 'BattleGround1':
                    self.nav = True
                    self.right, self.left, self.up, self.down = True, False, False, False
                    self.respawn = self.location
                    self.location = obj.name
                    self.nextMap = True

                elif obj.name == 'NextBattleMap':
                    self.nextMap = True

                elif obj.name == 'bossFight': # normal navigation for boss fight
                    if self.can_teleport and self.level <= self.bossCount:
                        self.nav = True
                        self.right, self.left, self.up, self.down = False, False, False, True
                        self.respawn = self.location
                        self.location = obj.name
                        self.can_return = False
                        self.can_teleport = False
                    elif self.endgame:
                        self.showMessage = True
                        self.message = 'Time to Back in time!'
                    else:
                        self.showMessage = True
                        self.message = 'Not now'

                elif obj.name == 'past':
                    if self.level > self.bossCount and self.endgame:
                        self.can_teleport = False
                        self.timeMachine = True
                    else:
                        self.showMessage = True
                        self.message = 'Not now'

            # reset the timer for transition in navigation
            if self.nav:
                self.loading_timer = self.loading_timer_max

            # self.walk = 0

    def navigate(self): # place the player to navigation / pointer object
        if self.nav:
            self.equiped1.duration = 0
            self.equiped2.duration = 0
            try:
                self.door.play()
                self.rect.x, self.rect.y = self.MapObjects[f'{self.respawn}_{self.location}'].rect.x, self.MapObjects[f'{self.respawn}_{self.location}'].rect.y
            except KeyError:
                self.rect.x, self.rect.y = 325, 225
        self.nav = False

    def loading_nav(self, screen):
        if self.loading and self.loading_timer > 0:
            self.can_move = False # avoid player to move during navigating to other map
            self.loading_timer -= 5
            pygame.draw.rect(screen, (0,0,0), (0, 0, 700, 500))
        else:
            self.can_move = True # make player can move again
            self.loading = False

    def pick(self, item, obj): # pick items from the chestBox
        keys = pygame.key.get_pressed()

        if keys[pygame.K_SPACE]:
            self.chestBoxSfx.play()
            if len(self.myWeapons) < 8:
                self.myWeapons.append(item)
                obj.loaded.remove(item)
            else:
                self.showMessage = True
                self.message = 'Inventory is already full'
                self.viewVaultBox = True
                print(f'Inventory is full - remain {len(item.name)}')

    def pickItems(self, items):
        for item in items:
            if self.rect.colliderect(item.x, item.y, 20, 20):
                if len(self.myWeapons) < 8:
                    self.pickItem_sfx.play()
                    self.myWeapons.append(item)
                    items.remove(item)

    def Message(self, screen):
        if self.showMessage:
            text = self.messageFont.render(self.message, True, (255,0,0))
            if not self.messageTimer.coolDown(5):
                screen.blit(text, (25, (500 - text.get_height()) / 2, text.get_width(), text.get_height()))
            else:
                self.showMessage = False
                    
    def openChestBox(self):
        keys = pygame.key.get_just_pressed()

        if keys[pygame.K_SPACE]:
            self.chestBoxSfx.play()
            self.viewInventory = True

    def openCraftBox(self):

        keys = pygame.key.get_just_pressed()

        if keys[pygame.K_SPACE]:
            self.chestBoxSfx.play()
            self.craft = True

    def openCollectable(self):
        keys = pygame.key.get_just_pressed()

        if keys[pygame.K_SPACE]:
            self.chestBoxSfx.play()
            self.collectables = True

    def showGuide(self): # view the guide from the monitor 1
        keys = pygame.key.get_just_pressed()
        foundKey = False

        if keys[pygame.K_SPACE]:
            self.sfx[0].play()
            for weapon in self.myWeapons:
                if weapon.code == 'icon14_09':
                    foundKey = True
                    self.openMonitor = True
                    break # break the loop

            if not foundKey:
                self.message = 'No Memory Card - find the memory card'
                self.showMessage = True

    def openBook(self):
        keys = pygame.key.get_just_pressed()

        if keys[pygame.K_SPACE]:
            self.sfx[0].play()
            self.openbook = True

    def timeTravel(self):
        if self.timeMachine:
            return True

    def handleFight(self, enemies=[]):
        # equiped 1
        self.equiped1.Trigger_mouse()
        self.equiped1.weapon(enemies)
        
        # equiped 2
        self.equiped2.Trigger_mouse2()
        self.equiped2.weapon(enemies)

    def masterWeapon(self):
        # if the player reach the score, the weapon that is from the other player recognize the player as its player
        for weapon in self.myWeapons:
            if not weapon.player:
                weapon.Master(self)

    def handleDefense(self, enemies=[]):
        # for sheild and other
        self.shield.Trigger()
        self.shield.weapon(enemies)

    def TriggerSkills(self, enemies=[]):
        self.CoolDown()
        self.skills.trigger()
        self.skills.skill(enemies)

    def CoolDown(self):
        if self.skill_cooldown < self.skills.skill_cooldown:
            self.skill_cooldown = self.timer.countDown
            if self.timer.coolDown(self.skills.skill_cooldown):
                self.skill_cooldown = self.skills.skill_cooldown

    def Attacked(self, enemy):
        if self.shieldPower > 0:
            self.shieldPower -= enemy.damage
        else:
            self.life -= enemy.damage

    def initSkill(self, screen):
        if self.name == 'johny':
            self.skills = skills.Boomerang(self, screen, (30,30))
            self.skill_cooldown = self.skills.skill_cooldown
        elif self.name == 'ricky':
            self.skills = skills.Copy(self, screen, (80, 80))
            self.skill_cooldown = self.skills.skill_cooldown
        elif self.name == 'jp':
            self.skills = skills.Shield(self, screen, (80, 80))
            self.skill_cooldown = self.skills.skill_cooldown
        elif self.name == 'jayson':
            self.skills = skills.Speed(self, screen)
            self.skill_cooldown = self.skills.skill_cooldown
        else:
            # raise error if player not found
            raise KeyError
        
    def Init(self, x, y):
        # player life
        self.defaultLife = 100
        self.defaultMana = 100
        self.life = self.defaultLife
        self.mana = self.defaultMana
        self.shieldPower = 0
        self.power = 10
        self.myWeapons = [] # for weapons
        self.equiped1 = None
        self.equiped2 = None
        self.potion = None
        self.shield = None
        self.level = 1 # debugging
        self.score = 0

        # skills
        self.skills = None
        self.skill_cooldown = None

        # speed
        self.speed = 7
        self.walk = 0

        # rect and surface
        self.rect = pygame.Rect((x, y), (self.width, self.height)) # for player rect

        # facing
        self.left = False
        self.right = True
        self.up = False
        self.down = False

        # image and animation
        self.c_left = []
        self.c_right = []
        self.c_up = []
        self.c_down = []

        # inventories / items list
        self.inventories = []
        self.collectedItems = []
        self.viewInventory = False
        self.viewVaultBox = False
        self.craft = False
        self.collectables = False
        self.openMonitor = False
        self.openbook = False

        # handling location
        self.location = 'base'
        self.loading = False
        self.loading_timer_max = 90
        self.loading_timer = 0
        self.can_move = True
        self.nextMap = False
        self.can_teleport = False # debugging
        self.can_return = True # for after navigating to boss fight map
        self.endgame = False # debugging
        self.timeMachine = False

        # # map objects
        self.nav = False
        self.respawn = 'base'

        # messages or notification
        self.showMessage = False
        self.message = None

# enemy variant 1
class Enemy(pygame.sprite.Sprite):

    def __init__(self, x, y, width, height, name=''):
        super().__init__()
        self.width = width
        self.height = height
        self.name = name

        # rect
        self.rect = pygame.Rect((x, y), (self.width, self.height))

        self.speed = None
        self.attacked = False # attacked by the player
        self.defaultLife = None
        self.life = 0
        self.shieldPower = 0
        self.damage = 0.5
        self.push = 0
        self.out = False
        self.pushed = False
        self.level = 1

        self.canFollow = True

        # facing
        self.left = True
        self.right = False
        self.up = False
        self.down = False
        self.walk = 0 # count of walk

        # images / character
        self.e_left = []
        self.e_right = []
        self.e_down = []
        self.e_up = []
        self.e_hit = None
        self.radius = 10

        # load and flip image
        self.loadImages()
        self.flipImage()

        # items
        self.items = []

        # sfx
        self.sfx = pygame.mixer.Sound('audio/hit.mp3')
        self.attack_sfx = pygame.mixer.Sound('audio/e_attack.mp3')
        self.sfx_timer = 0
        self.rawr_sfx = pygame.mixer.Sound('audio/e_sound.mp3')
        self.rawr_sfx_timer = 0

    # draw the enemy
    def draw(self, screen, objects):

        # handle collision
        self.handleCollision(objects)

        # for sound of enemies
        effect = random.choice(range(500))
        if effect == 0:
            if self.rawr_sfx_timer == 0:
                self.rawr_sfx_timer = 20
                self.rawr_sfx.play()
        if self.rawr_sfx_timer > 0:
            self.rawr_sfx_timer -= 1

        if self.sfx_timer > 0:
            self.sfx_timer -= 1
        
        if (self.walk + 1) >= 9:
            self.walk = 0

        if self.attacked:
            screen.blit(self.e_hit, self.rect)
            self.attacked = False
            self.walk = 0

        elif self.left:
            if self.rect.x > -self.width and self.rect.x < 700 and self.rect.y > -self.height and self.rect.y < 500:
                screen.blit(self.e_left[self.walk//3], (self.rect.x, self.rect.y))
            self.walk += 1
        elif self.up:
            if self.rect.x > -self.width and self.rect.x < 700 and self.rect.y > -self.height and self.rect.y < 500:
                screen.blit(self.e_up[self.walk//3], (self.rect.x, self.rect.y))
            self.walk += 1
        elif self.right:
            if self.rect.x > -self.width and self.rect.x < 700 and self.rect.y > -self.height and self.rect.y < 500:
                screen.blit(self.e_right[self.walk//3], (self.rect.x, self.rect.y))
            self.walk += 1
        elif self.down:
            if self.rect.x > -self.width and self.rect.x < 700 and self.rect.y > -self.height and self.rect.y < 500:
                screen.blit(self.e_down[self.walk//3], (self.rect.x, self.rect.y))
            self.walk += 1
        else:
            if self.rect.x > -self.width and self.rect.x < 700 and self.rect.y > -self.height and self.rect.y < 500:
                if self.left:
                    screen.blit(self.e_left[0], (self.rect.x, self.rect.x))
                elif self.right:
                    screen.blit(self.e_right[0], self.rect.x, self.rect.y)

        # pygame.draw.rect(screen, (255, 255, 255), self.rect, 1)

        self.barLife(screen)

    def barLife(self, screen):
        pygame.draw.rect(screen, (207, 208, 255), (self.rect.x, self.rect.y - (self.height / 2) + 10, self.width, 5))
        pygame.draw.rect(screen, (252, 78, 15), (self.rect.x, self.rect.y - (self.height / 2) + 10, (self.life / self.defaultLife) * self.width, 5))

    # enemy x and y move directions
    def move_x(self, direction):
        self.rect.x += direction

    def move_y(self, direction):
        self.rect.y += direction

    def hit_effects(self, screen):
        if self.radius <= 90:
            pygame.draw.circle(screen, (255,255,255, 20), self.rect.center, float(self.radius))
            self.radius += 30
        else:
            self.radius = 10
            self.out = True

    # enemy follow player until player's life is 0
    def follow(self, player): # for zombies and slimes
        if player.life > 0 and not(self.pushed) and self.canFollow:
            if self.rect.x > player.rect.x + 35:
                self.left = True
                self.right = False
                self.up = False
                self.down = False
                self.move_x(self.speed * -1)
            elif self.rect.x < player.rect.x - 35:
                self.left = False
                self.right = True
                self.up = False
                self.down = False
                self.move_x(self.speed)
            elif self.rect.y > player.rect.y + 35:
                self.left = False
                self.right = False
                self.up = True
                self.down = False
                self.move_y(self.speed * -1)
            elif self.rect.y < player.rect.y - 35:
                self.left = False
                self.right = False
                self.up = False
                self.down = True
                self.move_y(self.speed)

    def Attack(self, player):
        if pygame.sprite.collide_rect(self, player):
            player.Attacked(self)
            if self.sfx_timer == 0:
                self.sfx_timer = 50
                self.attack_sfx.play()
        if self.sfx_timer > 0:
            self.sfx_timer -= 1

    # load enemies image
    def loadImages(self):
        sides = ['D_Walk_', 'U_Walk_', 'S_Walk_']
        for side in sides:
            for i in range(3):
                image = f'characters/zombies/{side}{i}.png'
                image = pygame.image.load(image)
                image = pygame.transform.scale(image, (self.width, self.height))
                if side == sides[0]:
                    self.e_down.append(image)
                elif side == sides[1]:
                    self.e_up.append(image)
                elif side == sides[2]:
                    self.e_right.append(image)

        # load the hit animation
        image = f'characters/zombies/hit.png'
        image = pygame.image.load(image)
        image = pygame.transform.scale(image, (self.width, self.height))
        self.e_hit = image

    # flip the image of enemy
    def flipImage(self):
        for character in self.e_right:
            self.e_left.append(pygame.transform.flip(character, True, False))

    # enemies collision
    def handleCollision(self, objects):
        for object in objects:
            if object._type not in ['hidden2', 'other2', 'navigation', 'animated2']:
                if self.left or self.right:
                    if self.rect.y > object.rect.y:
                        object.e_front.append(self)
                    elif self.rect.y <= object.rect.y:
                        if self in object.e_front:
                            object.e_front.remove(self)
            if pygame.sprite.collide_rect(self, object):
                if object._type not in ['hidden2', 'other2', 'navigation', 'animated2']:
                    if self.left:
                        if self.rect.y <= object.rect.y:
                            self.rect.left = object.rect.right
                    elif self.right:
                        if self.rect.y <= object.rect.y:
                            self.rect.right = object.rect.left

                    elif self.up and self in object.e_front:
                        if self.rect.top < object.rect.bottom - 40:
                            self.rect.top = object.rect.bottom - 40
                    elif self.down and self not in object.e_front:
                        self.rect.bottom = object.rect.top
                else:
                    if object._type not in ['navigation', 'other2', 'animated2']:
                        if self.right:
                            self.rect.right = object.rect.left
                        elif self.left:
                            self.rect.left = object.rect.right
                        elif self.up:
                            self.rect.top = object.rect.bottom
                        elif self.down:
                            self.rect.bottom = object.rect.top

# types of Enemies
class Zombie(Enemy):

    def __init__(self, x, y, width, height, name='zombie'):
        super().__init__(x, y, width, height, name)

class Slime(Enemy):

    def __init__(self, x, y, width, height, name='slime'):
        super().__init__(x, y, width, height, name)

class Robot(Enemy):

    def __init__(self, x, y, width, height, name='robots'):
        super().__init__(x, y, width, height, name)

class NPC(pygame.sprite.Sprite):

    def __init__(self, x, y, width, height, name=''):
        super().__init__()
        self.width = width
        self.height = height
        self.name = name

        self.rect = pygame.Rect(x, y, self.width, self.height)
        self.image = pygame.Surface((self.width, self.height))

        self.c_left = []
        self.c_right = []
        self.c_up = []
        self.c_down = []
        if self.name != '':
            self.loadImages()
            self.flip()

    def draw(self, screen):
        if self.name != '':
            screen.blit(self.c_down[0], self.rect)
        else:
            pygame.draw.rect(screen, (255,255,255), self.rect)

    def loadImages(self):
        sides = ['D_Walk_', 'S_Walk_', 'U_Walk_']

        for side in sides:
            for i in range(7):
                image = f'characters/NPC/{self.name}/{side}{i}.png'
                image = pygame.image.load(image)
                image = pygame.transform.scale(image, (self.width, self.height))
                if side == sides[0]:
                    self.c_down.append(image)
                elif side == sides[1]:
                    self.c_left.append(image)
                elif side == sides[2]:
                    self.c_up.append(image)
                else:
                    print('not found')

    def flip(self):
        for image in self.c_left:
            c_right = pygame.transform.flip(image, True, False)
            self.c_right.append(c_right)